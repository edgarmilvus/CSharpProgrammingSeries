
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

// Source File: solution_exercise_6.cs
// Description: Solution for Exercise 6
// ==========================================

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Resilience;
using Polly.CircuitBreaker;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Resilience (Polly-based)
builder.Services.AddResiliencePipeline("llm-pipeline", builder =>
{
    builder
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5, // 50% failure ratio
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5, // Minimum 5 requests in the window
            BreakDuration = TimeSpan.FromSeconds(30),
            OnOpened = args =>
            {
                Console.WriteLine($"Circuit opened! Reason: {args.BreakDuration}");
                return ValueTask.CompletedTask;
            }
        })
        .AddTimeout(TimeSpan.FromSeconds(15)); // Global timeout per request
});

// 2. Configure Chat Client with Fallback
// We use IChatClient from Microsoft.Extensions.AI
builder.Services.AddChatClient(sp =>
{
    // Primary Client (Ollama)
    var primaryClient = new OllamaChatClient("http://localhost:11434", "llama2");
    
    // Wrap with Resilience
    var pipeline = sp.GetRequiredService<IResiliencePipelineProvider>()
        .GetPipeline<HttpResponseMessage>("llm-pipeline");

    // Note: Microsoft.Extensions.Resilience integration with IChatClient 
    // requires adapting the pipeline. For this example, we simulate the wrapper logic
    // or use a typed client approach. Here is a simplified wrapper approach:
    
    return new ResilientChatClient(primaryClient, pipeline);
});

var app = builder.Build();

// 3. Minimal API Endpoint
app.MapPost("/api/chat", async (IChatClient chatClient, ChatRequest request) =>
{
    try
    {
        var response = await chatClient.GetResponseAsync(request.Prompt);
        return Results.Ok(new { response.Text });
    }
    catch (BrokenCircuitException)
    {
        // Circuit is open
        return Results.Ok(new { response = "I am currently busy (Circuit Open). Please try again later." });
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred: {ex.Message}");
    }
});

// Health Check Endpoint (for K8s liveness)
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();

// Supporting Types
public record ChatRequest(string Prompt);

// Custom Wrapper to handle Fallback logic inside the resilience pipeline
public class ResilientChatClient : IChatClient
{
    private readonly IChatClient _innerClient;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public ResilientChatClient(IChatClient innerClient, ResiliencePipeline<HttpResponseMessage> pipeline)
    {
        _innerClient = innerClient;
        _pipeline = pipeline;
    }

    public async Task<ChatResponse> GetResponseAsync(string prompt, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        // Execute the LLM call within the resilience pipeline
        var result = await _pipeline.ExecuteAsync(async token =>
        {
            // In a real scenario, we need to bridge IChatClient to an HttpResponseMessage 
            // for Polly, or use Polly's generic ExecuteAsync.
            // Since IChatClient throws exceptions directly, we catch them here for the fallback.
            try 
            {
                var response = await _innerClient.GetResponseAsync(prompt, options, token);
                // Return a dummy HttpResponseMessage to satisfy the pipeline signature if strictly required,
                // or adapt the pipeline to handle generic tasks.
                // For this exercise, we assume the pipeline handles the execution context.
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("Success") };
            }
            catch (Exception)
            {
                throw; // Let Polly handle the exception
            }
        }, cancellationToken);

        // If we reach here, the pipeline executed successfully.
        // Re-run the actual logic (or cache the result) - 
        // In a robust implementation, the resilience policy wraps the actual async delegate.
        
        // Fallback Logic:
        // If the pipeline throws (CircuitBreaker), we catch it in the endpoint.
        // To implement a Fallback *inside* the pipeline:
        return await _innerClient.GetResponseAsync(prompt, options, cancellationToken);
    }

    // Implement other interface members...
    public Task<ChatResponse<T>> GetResponseAsync<T>(string prompt, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
    
    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(string prompt, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
