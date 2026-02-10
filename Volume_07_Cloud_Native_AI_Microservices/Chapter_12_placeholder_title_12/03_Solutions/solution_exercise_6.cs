
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

// Orchestrator Program.cs
using Polly;
using Polly.Timeout;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServiceTracing("Orchestrator");
builder.Services.AddHttpClient("Researcher", client => 
{
    client.BaseAddress = new Uri(builder.Configuration["RESEARCHER_URL"]);
});

var app = builder.Build();

// Define Policy
var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(2), TimeoutStrategy.Pessimistic);
var fallbackPolicy = Policy<string>
    .Handle<Exception>()
    .Or<TimeoutRejectedException>()
    .FallbackAsync(
        fallbackValue: "Fallback summary due to researcher timeout",
        onFallback: (ctx, ex) => 
        {
            // Log the fallback event
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Researcher timed out. Returning fallback.");
            return Task.CompletedTask;
        }
    );

app.MapPost("/query", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    // 1. Create Custom Span
    using var activity = Activity.Current?.Source.StartActivity("Orchestrator-Workflow");
    activity?.SetTag("user.id", "12345");
    activity?.SetTag("query.length", 100);

    var client = clientFactory.CreateClient("Researcher");
    
    // 2. Combine Policies (Timeout + Fallback)
    var policyWrap = Policy.WrapAsync(fallbackPolicy, timeoutPolicy);

    string researchResult = await policyWrap.ExecuteAsync(async () =>
    {
        // This call automatically propagates traceparent headers
        var response = await client.GetAsync("/research");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    });

    // Continue to Summarizer (omitted for brevity, follows same pattern)
    
    return Results.Ok(new { summary = researchResult });
});

app.Run();
