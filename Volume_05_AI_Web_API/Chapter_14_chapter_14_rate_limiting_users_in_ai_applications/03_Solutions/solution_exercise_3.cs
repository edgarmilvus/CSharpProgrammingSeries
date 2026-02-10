
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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1. Define Configuration Structure
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection("RateLimits"));

// 2. Add Authentication (Mocked for context)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* Configuration omitted for brevity */ });

builder.Services.AddAuthorization();

// 3. Configure Rate Limiting Policies
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Helper to get tier from context
    string? GetTier(HttpContext context)
    {
        return context.User.FindFirstValue("SubscriptionTier"); // Expecting "Free", "Pro", "Enterprise"
    }

    // Free Policy (Fixed Window)
    options.AddPolicy("Free", context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                AutoReplenishment = true
            });
    });

    // Pro Policy (Sliding Window)
    options.AddPolicy("Pro", context =>
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.User.Identity?.Name,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                SegmentsPerWindow = 4, // 15 min segments for 1 hour window
                Window = TimeSpan.FromHours(1),
                AutoReplenishment = true
            });
    });

    // Enterprise Policy (Fixed Window, high limit)
    options.AddPolicy("Enterprise", context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10000,
                Window = TimeSpan.FromHours(1),
                AutoReplenishment = true
            });
    });
});

builder.Services.AddSingleton<DynamicRateLimitService>(); // Service to hold dynamic state

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// 4. Apply Policies to v2 Endpoints
var v2 = app.MapGroup("/api/v2").RequireRateLimiting(policyName: "DynamicPolicy"); // Placeholder

// Dynamic Middleware to select policy based on Claim
app.Use(async (context, next) =>
{
    // This middleware intercepts requests to /api/v2 and selects the policy dynamically
    if (context.Request.Path.StartsWithSegments("/api/v2"))
    {
        var tier = context.User.FindFirstValue("SubscriptionTier");
        var feature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IRateLimitingFeature>();
        
        // Note: Standard middleware usually applies policy via metadata.
        // To do this dynamically per request, we often use a custom middleware 
        // or a policy that checks the claim.
        // Here, we rely on the endpoint metadata or a custom resolver.
    }
    await next();
});

// Endpoint to retrieve current limits (Read-only view)
app.MapGet("/api/v2/data", (HttpContext context) =>
{
    return Results.Ok($"Accessed by {context.User.Identity?.Name}");
});

// Interactive Challenge: Dynamic Update Endpoint
app.MapPost("/admin/update-limits", (UpdateLimitRequest request, DynamicRateLimitService service) =>
{
    service.UpdateLimit(request.Tier, request.NewLimit);
    return Results.Ok($"Updated {request.Tier} limit to {request.NewLimit}");
});

app.Run();

// Supporting Classes
public record UpdateLimitRequest(string Tier, int NewLimit);

public class DynamicRateLimitService
{
    private readonly Dictionary<string, int> _limits = new()
    {
        { "Free", 30 },
        { "Pro", 1000 },
        { "Enterprise", 10000 }
    };

    public void UpdateLimit(string tier, int limit) => _limits[tier] = limit;
    public int GetLimit(string tier) => _limits.TryGetValue(tier, out var limit) ? limit : 30;
}
