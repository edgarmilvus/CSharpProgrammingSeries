
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;

public class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpLoggingHandler> _logger;

    public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // 1. Log Request Details
        await LogRequest(request);

        // Execute the request
        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request failed after {Duration}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }

        stopwatch.Stop();

        // 2. Log Response Details
        _logger.LogInformation(
            "Response: {StatusCode} | Duration: {Duration}ms | ContentLength: {Length}",
            (int)response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            response.Content.Headers.ContentLength ?? 0
        );

        return response;
    }

    private async Task LogRequest(HttpRequestMessage request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Request: {request.Method} {request.RequestUri}");
        
        // 3. Redact Sensitive Headers
        sb.AppendLine("Headers:");
        foreach (var header in request.Headers)
        {
            if (header.Key.ToLower().Contains("authorization") || header.Key.ToLower().Contains("api-key"))
            {
                sb.AppendLine($"  {header.Key}: [REDACTED]");
            }
            else
            {
                sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        // 4. Log Body (Handle Stream safely)
        if (request.Content != null)
        {
            // Buffer the content to read it without consuming the stream permanently
            // Note: For very large payloads, this increases memory usage. 
            // For strict streaming, we would need to wrap the stream, but for logging, buffering is standard.
            var content = await request.Content.ReadAsStringAsync();
            
            // Limit length
            var displayContent = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
            sb.AppendLine($"Body: {displayContent}");
        }

        _logger.LogInformation(sb.ToString());
    }
}

// Registration in Program.cs
public static class ServiceRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // 4. Ensure handler is added before resilience policies
        services.AddTransient<HttpLoggingHandler>();

        services.AddHttpClient<OpenAiChatClient>()
            .AddHttpMessageHandler<HttpLoggingHandler>() // Adds logging
            .AddPolicyHandler(PolicyHelper.GetRetryPolicy(/* logger */)); // Adds resilience
    }
}
