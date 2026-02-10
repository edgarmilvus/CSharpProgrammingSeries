
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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text.Json;
using BCrypt.Net; // Assuming BCrypt.Net is installed for hashing

// 1. API Key Store Interface and Implementation
public interface IApiKeyStore
{
    Task<ApiKeyValidationResult?> ValidateKeyAsync(string apiKey);
}

public class ApiKeyValidationResult
{
    public string KeyId { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class InMemoryApiKeyStore : IApiKeyStore
{
    // In production, this would query a database.
    // We simulate a DB where keys are hashed.
    private static readonly Dictionary<string, ApiKeyValidationResult> _keyStore = new()
    {
        // Key: "sk_test_123" -> Hash: $2a$11$... (BCrypt hash)
        // We map the Hash to the Result
        { "$2a$11$EixZaYVK1fsbw1ZfbX3OXePaWxn96p36WQoeG6Lruj3vjPGga31lW", new ApiKeyValidationResult { KeyId = "key_1", Permissions = new List<string> { "Model.Read", "Chat.Write" } } }
    };

    public Task<ApiKeyValidationResult?> ValidateKeyAsync(string apiKey)
    {
        // In a real scenario, we would iterate the DB and check BCrypt.Verify(apiKey, storedHash)
        // For simulation, we verify against the hardcoded hash for "sk_test_123"
        var isValid = BCrypt.Net.BCrypt.Verify(apiKey, "$2a$11$EixZaYVK1fsbw1ZfbX3OXePaWxn96p36WQoeG6Lruj3vjPGga31lW");
        
        if (isValid)
        {
            return Task.FromResult<ApiKeyValidationResult?>(_keyStore.Values.First());
        }
        return Task.FromResult<ApiKeyValidationResult?>(null);
    }
}

// 2. Custom Attribute for Route Constraints
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresApiKeyAttribute : Attribute
{
    // This attribute acts as a marker for the middleware
}

// 3. Hybrid Auth Middleware
public class HybridAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public HybridAuthMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyStore apiKeyStore)
    {
        // Skip if already authenticated (e.g. by a previous middleware)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        bool isApiKeyEndpoint = context.GetEndpoint()?.Metadata.GetMetadata<RequiresApiKeyAttribute>() != null;

        // 1. Check for Authorization Header (JWT)
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // If the endpoint requires API Key specifically, reject JWT
            if (isApiKeyEndpoint)
            {
                context.Response.StatusCode = 403; // Forbidden for this method
                await context.Response.WriteAsync("This endpoint requires an API Key, not a JWT.");
                return;
            }

            // Validate JWT (Delegating to the standard ASP.NET Core JWT Bearer Handler configured in Program.cs)
            // We simply pass through; the standard Auth Middleware usually runs before this, 
            // but if we are building a custom pipeline, we trigger the default scheme.
            await _next(context);
            return;
        }

        // 2. Check for API Key (Header or Query Param for WebSockets)
        string? apiKey = null;

        // Priority 1: Header
        if (context.Request.Headers.TryGetValue("X-API-Key", out var headerKey))
        {
            apiKey = headerKey.ToString();
        }
        // Priority 2: Query Parameter (Only for WebSocket upgrade requests or specific config)
        else if (context.Request.Query.TryGetValue("apikey", out var queryKey))
        {
            // Security Check: Only allow query param for WebSocket upgrades or specific internal routes
            if (context.Request.Headers["Upgrade"].Equals("websocket"))
            {
                apiKey = queryKey.ToString();
            }
        }

        if (!string.IsNullOrEmpty(apiKey))
        {
            // Check Cache
            var cacheKey = $"apikey_{apiKey.GetHashCode()}";
            ApiKeyValidationResult? validationResult = null;

            if (!_cache.TryGetValue(cacheKey, out validationResult))
            {
                validationResult = await apiKeyStore.ValidateKeyAsync(apiKey);
                if (validationResult != null)
                {
                    // Cache for 5 minutes
                    _cache.Set(cacheKey, validationResult, TimeSpan.FromMinutes(5));
                }
            }

            if (validationResult != null)
            {
                // Construct ClaimsPrincipal for API Key
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, validationResult.KeyId),
                    new Claim("auth_method", "api_key")
                };

                // Map permissions to Roles
                foreach (var perm in validationResult.Permissions)
                {
                    claims.Add(new Claim(ClaimTypes.Role, perm));
                }

                var identity = new ClaimsIdentity(claims, "ApiKey");
                var principal = new ClaimsPrincipal(identity);

                // Attach to HttpContext
                context.User = principal;

                // Check Policy Constraints (e.g., UserOnly)
                // If the endpoint requires a policy that API Keys don't satisfy, we could fail here.
                // For this exercise, we assume standard [Authorize] works for both unless specified otherwise.
                
                await _next(context);
                return;
            }
        }

        // 3. Not Authenticated
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Authentication required.");
    }
}
