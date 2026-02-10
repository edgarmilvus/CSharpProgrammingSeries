
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. OpenTelemetry Configuration
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource("AIAgent.Orchestrator")
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter());

// 2. Register Service Bus Client
var serviceBusConnString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION");
builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnString));

// 3. Register Message Publisher
builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();

var app = builder.Build();

app.MapPost("/orchestrate", async (InferenceRequest request, IMessagePublisher publisher) =>
{
    // Create an Activity to represent the request scope
    using var activity = Activity.Current?.Source.StartActivity("OrchestrateInference", ActivityKind.Server);
    
    // Publish message
    await publisher.PublishAsync(request);
    
    // Return 202 Accepted immediately
    return Results.Accepted($"/status/{request.Id}");
});

app.Run();

// --- Supporting Classes ---

public interface IMessagePublisher
{
    Task PublishAsync(InferenceRequest request);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly ServiceBusSender _sender;
    private static readonly ActivitySource ActivitySource = new("AIAgent.Orchestrator");

    public MessagePublisher(ServiceBusClient client)
    {
        _sender = client.CreateSender("inference-queue");
    }

    public async Task PublishAsync(InferenceRequest request)
    {
        using var activity = ActivitySource.StartActivity("PublishMessage", ActivityKind.Producer);
        
        var message = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(request));
        
        // 4. Propagate Trace Context
        if (activity != null)
        {
            message.ApplicationProperties["traceparent"] = activity.Id;
            // You can also add baggage if needed
        }

        await _sender.SendMessageAsync(message);
    }
}
