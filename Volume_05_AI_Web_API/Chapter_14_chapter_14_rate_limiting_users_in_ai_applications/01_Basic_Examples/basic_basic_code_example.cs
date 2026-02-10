
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Services
// We register the rate limiting services and define a policy named "chatbot_policy".
builder.Services.AddRateLimiter(options =>
{
    // Define a Token Bucket limiter.
    // Token Bucket is ideal for AI APIs because it allows for "bursts" of requests
    // (e.g., a user sending multiple rapid messages) while maintaining a steady average limit.
    options.AddPolicy<string>("chatbot_policy", context =>
    {
        // Retrieve the user's identity from the request.
        // In a real app, this would be an API Key or User ID from a header or JWT claim.
        var userId = context.User.Identity?.Name ?? "anonymous";

        // Configure the Token Bucket options:
        // - PermitLimit: The maximum number of tokens the bucket can hold (the burst size).
        // - QueueProcessingOrder: Oldest requests are processed first.
        // - QueueLimit: How many requests to queue if the bucket is empty (0 for immediate rejection).
        // - ReplenishmentPeriod: How often tokens are added.
        // - TokensPerPeriod: How many tokens are added per replenishment period.
        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: userId,
            factory: _ => new TokenBucketRateLimiterOptions
            {
                PermitLimit = 10,            // Allow 10 requests in a burst
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,              // Do not queue requests if limit is hit
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                TokensPerPeriod = 2          // Add 2 tokens per second (avg 2 RPS)
            });
    });
});

var app = builder.Build();

// 2. Enable Rate Limiting Middleware
// This activates the middleware globally. Without this, policies are registered but not enforced.
app.UseRateLimiter();

// 3. Define the AI Chat Endpoint
app.MapPost("/api/chat", async (HttpContext context, Request request) =>
{
    // Simulate AI processing latency
    await Task.Delay(100);

    return Results.Ok(new { 
        Response = $"AI Response to: {request.Message}", 
        User = context.User.Identity?.Name ?? "Anonymous" 
    });
})
.WithName("ChatEndpoint")
.RequireRateLimiting("chatbot_policy"); // Apply the specific policy to this endpoint

// Helper class for the request body
public record Request(string Message);

app.Run();

// 4. Configuration for Kestrel (Optional but recommended for production)
// This ensures the server doesn't hang indefinitely on rate-limited connections.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
});
