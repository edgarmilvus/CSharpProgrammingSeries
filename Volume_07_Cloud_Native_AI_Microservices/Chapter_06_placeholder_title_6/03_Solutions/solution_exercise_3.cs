
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

// File: Program.cs (Downstream Call Logic)
using Polly; // NuGet: Polly
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole();

// HttpClient Registration with Polly Retry Policy
builder.Services.AddHttpClient("LoggingService", client =>
{
    // URL comes from K8s Service DNS
    client.BaseAddress = new Uri("http://logging-service");
})
.AddPolicyHandler(Policy<HttpResponseMessage>
    .Handle<HttpRequestException>()
    .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
    onRetry: (outcome, timespan, retryCount, context) =>
    {
        Console.WriteLine($"Retry {retryCount} for {outcome.Result?.StatusCode}. Waiting {timespan.TotalSeconds}s");
    }));

var app = builder.Build();

app.MapPost("/analyze", async (AnalysisRequest request, IHttpClientFactory httpClientFactory) =>
{
    // 1. Perform Inference (Mocked)
    var result = new { Sentiment = "Positive" };

    // 2. Call Downstream Logging Service (Resilient)
    try
    {
        var client = httpClientFactory.CreateClient("LoggingService");
        // In Istio, we can rely on mTLS automatically handled by sidecar
        // But we must ensure we use the Service Name, not IP
        var response = await client.PostAsJsonAsync("/log", new { request.Text, result.Sentiment });
        
        if (!response.IsSuccessStatusCode)
        {
            // Log failure but don't fail the main request
            Console.WriteLine("Failed to log to downstream service.");
        }
    }
    catch (Exception ex)
    {
        // Circuit breaker logic could go here
        Console.WriteLine($"Critical error calling logging service: {ex.Message}");
    }

    return Results.Ok(result);
});

app.Run();

public record AnalysisRequest(string Text);
