
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
#
# MIT License
# Copyright (c) 2026 Edgar Milvus
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
*/

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Polly;
using Polly.CircuitBreaker;
using System.Diagnostics;
using System.Text.Json;

public class WorkerService : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IAsyncPolicy<string> _circuitBreakerPolicy;
    private readonly ILogger<WorkerService> _logger;
    private static readonly ActivitySource ActivitySource = new("AIAgent.Worker");

    public WorkerService(ServiceBusClient client, ILogger<WorkerService> logger)
    {
        _logger = logger;
        
        // 2. Circuit Breaker Configuration
        _circuitBreakerPolicy = Policy<string>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) => 
                    _logger.LogWarning($"Circuit broken for {breakDelay.TotalSeconds}s due to {ex.Message}"),
                onReset: () => _logger.LogInformation("Circuit reset")
            );

        _processor = client.CreateProcessor("inference-queue", new ServiceBusProcessorOptions { MaxConcurrentCalls = 4 });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += async args =>
        {
            // 3. Extract Trace Context
            var traceParent = args.Message.ApplicationProperties.TryGetValue("traceparent", out var tp) ? tp.ToString() : null;
            
            using var activity = ActivitySource.StartActivity("ProcessInference", ActivityKind.Consumer);
            
            if (traceParent != null && activity != null)
            {
                // Restore the context from the message headers
                var context = new ActivityContext(ActivityTraceId.CreateFromString(traceParent.Substring(3, 32)), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
                activity.SetParentId(context.TraceId);
            }

            try
            {
                var request = JsonSerializer.Deserialize<InferenceRequest>(args.Message.Body.ToString());
                
                // Execute external API call with Circuit Breaker
                var result = await _circuitBreakerPolicy.ExecuteAsync(() => CallExternalModelApi(request.Text));
                
                _logger.LogInformation($"Processed {request.Id}: {result}");
                
                await args.CompleteMessageAsync(args.Message);
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit is open. Request failed immediately.");
                // Dead letter or retry later
                await args.AbandonMessageAsync(args.Message);
            }
        };

        _processor.ProcessErrorAsync += args => { _logger.LogError(args.Exception.ToString()); return Task.CompletedTask; };

        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task<string> CallExternalModelApi(string text)
    {
        // Simulate unreliable external API
        using var client = new HttpClient();
        // ... call external API
        return await Task.FromResult("Positive");
    }
}
