
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

// --- CLIENT SIDE SIMULATION ---
public class HmacRequestSigner
{
    private readonly string _clientId;
    private readonly string _clientSecret;

    public HmacRequestSigner(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    public void SignRequest(HttpRequestMessage request, string content)
    {
        var timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601
        request.Headers.Add("X-Client-Id", _clientId);
        request.Headers.Add("X-Timestamp", timestamp);

        // Construct the string to sign
        // Format: Method + ContentType + Timestamp + Body
        var stringToSign = $"{request.Method.Method}\n" +
                           $"{request.Content?.Headers.ContentType?.MediaType ?? ""}\n" +
                           $"{timestamp}\n" +
                           $"{content}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_clientSecret));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
        var signature = Convert.ToBase64String(signatureBytes);

        request.Headers.Add("X-Signature", signature);
    }
}

// --- SERVER SIDE MIDDLEWARE ---
public class HmacValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    // In production, inject a service to look up secrets by ClientId
    private readonly Dictionary<string, string> _secrets = new() { { "client_1", "super_secret_key_123" } };

    public HmacValidationMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate if the endpoint requires HMAC (e.g., marked by attribute or specific path)
        // For this exercise, we assume it runs globally but checks for headers
        
        if (!context.Request.Headers.ContainsKey("X-Signature") || 
            !context.Request.Headers.ContainsKey("X-Client-Id") ||
            !context.Request.Headers.ContainsKey("X-Timestamp"))
        {
            await _next(context);
            return;
        }

        var clientId = context.Request.Headers["X-Client-Id"].ToString();
        var timestampStr = context.Request.Headers["X-Timestamp"].ToString();
        var receivedSignature = context.Request.Headers["X-Signature"].ToString();

        // 1. Replay Prevention: Check Timestamp
        if (!DateTime.TryParse(timestampStr, out var timestamp) || 
            (DateTime.UtcNow - timestamp).TotalMinutes > 5)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid or expired timestamp");
            return;
        }

        // Check cache for duplicate timestamp (Replay attack)
        var replayKey = $"hmac_{clientId}_{timestampStr}";
        if (_cache.ContainsKey(replayKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Request replay detected");
            return;
        }

        // 2. Lookup Secret
        if (!_secrets.TryGetValue(clientId, out var secret))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid Client ID");
            return;
        }

        // 3. Reconstruct Signature
        // Note: Reading the request body modifies the stream. We need to enable buffering.
        context.Request.EnableBuffering();
        string body;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset for downstream middleware
        }

        var stringToSign = $"{context.Request.Method}\n" +
                           $"{context.Request.ContentType}\n" +
                           $"{timestampStr}\n" +
                           $"{body}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
        var computedSignature = Convert.ToBase64String(computedHash);

        // 4. Constant-Time Comparison
        if (!CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(receivedSignature), 
            Convert.FromBase64String(computedSignature)))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid Signature");
            return;
        }

        // 5. Cache Timestamp (Sliding window 5 mins)
        _cache.Set(replayKey, true, TimeSpan.FromMinutes(5));

        await _next(context);
    }
}
