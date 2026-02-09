
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

using Microsoft.AspNetCore.Mvc;
using System.Net;

// Custom Exception
public class ContentFilteredException : Exception
{
    public string FilterResult { get; }
    public string Severity { get; }

    public ContentFilteredException(string filterResult, string severity)
    {
        FilterResult = filterResult;
        Severity = severity;
    }
}

// Middleware Implementation
public class ContentFilterMiddleware
{
    private readonly RequestDelegate _next;

    public ContentFilterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ContentFilteredException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Type = "https://yourdomain.com/errors/content-filter",
                Title = "Content Violation",
                Status = 400,
                Detail = "Your request was rejected due to content policy violations.",
                Extensions = 
                {
                    ["filterResult"] = ex.FilterResult,
                    ["severity"] = ex.Severity
                }
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

// Extension method for clean registration
public static class ContentFilterMiddlewareExtensions
{
    public static IApplicationBuilder UseContentFilter(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ContentFilterMiddleware>();
    }
}
