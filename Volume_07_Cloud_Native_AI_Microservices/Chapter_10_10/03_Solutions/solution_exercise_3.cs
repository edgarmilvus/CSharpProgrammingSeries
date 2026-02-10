
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class MeshSecurityHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Propagate a Trace ID (X-Request-ID)
        string traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
        
        // Remove existing if any to simulate fresh propagation or overwrite
        if (request.Headers.Contains("X-Request-ID"))
        {
            request.Headers.Remove("X-Request-ID");
        }
        request.Headers.Add("X-Request-ID", traceId);

        // 2. Simulate adding a Client Certificate
        // In a real scenario, we would attach a certificate to the HttpClientHandler.
        // Here we simulate the concept by adding a custom property (legacy) or just logging.
        // Modern .NET suggests using HttpClientHandler.ClientCertificates.
        // For the purpose of this exercise, we ensure the request is marked as secure.
        if (request.Properties.ContainsKey("IsSecure"))
        {
            request.Properties["IsSecure"] = true;
        }
        else
        {
            request.Properties.Add("IsSecure", true);
        }

        // Log the simulation
        // _logger.LogInformation($"Sending request with TraceID: {traceId} and simulated mTLS");

        return await base.SendAsync(request, cancellationToken);
    }
}

public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TracingMiddleware> _logger;

    public TracingMiddleware(RequestDelegate next, ILogger<TracingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Check for X-Request-ID header
        string traceId = null;
        if (context.Request.Headers.TryGetValue("X-Request-ID", out var headerValues))
        {
            traceId = headerValues.ToString();
        }

        Activity activity = null;

        if (!string.IsNullOrEmpty(traceId))
        {
            // 2. If present, set Activity.Current.TraceId
            // We create a new Activity linked to the incoming trace
            activity = new Activity("InboundRequest");
            activity.SetParentId(traceId); // Linking to the parent trace
            activity.Start();
            
            // Set baggage if needed
            context.Items["TraceId"] = traceId;
        }
        else
        {
            // 3. If missing, start a new Activity (Root of a trace)
            activity = new Activity("InboundRequest");
            activity.Start();
            context.Items["TraceId"] = activity.TraceId.ToString();
            _logger.LogWarning("Missing X-Request-ID header. Generated new Trace ID.");
        }

        try
        {
            await _next(context);
        }
        finally
        {
            activity?.Stop();
        }
    }
}
