
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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace InferenceMetrics
{
    public class InferenceMetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private static long _activeRequests = 0;
        private static long _totalRequests = 0;

        public InferenceMetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Expose metrics endpoint
            if (context.Request.Path.StartsWithSegments("/metrics"))
            {
                // Prometheus format
                var metrics = $"""
                    # HELP inference_active_requests Number of active inference requests
                    # TYPE inference_active_requests gauge
                    inference_active_requests {_activeRequests}

                    # HELP inference_total_requests Total number of inference requests
                    # TYPE inference_total_requests counter
                    inference_total_requests {_totalRequests}
                    """;
                
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(metrics);
                return;
            }

            // Track metrics for other requests
            Interlocked.Increment(ref _activeRequests);
            Interlocked.Increment(ref _totalRequests);
            try
            {
                await _next(context);
            }
            finally
            {
                Interlocked.Decrement(ref _activeRequests);
            }
        }
    }

    // Extension method to register middleware easily
    public static class InferenceMetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UseInferenceMetrics(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InferenceMetricsMiddleware>();
        }
    }
}
