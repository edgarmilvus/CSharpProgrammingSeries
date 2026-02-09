
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

// Custom exception for Exercise 1 Interactive Challenge
public class ModelLoadException : Exception
{
    public string ModelName { get; }

    public ModelLoadException(string modelName, string message) : base(message)
    {
        ModelName = modelName;
    }
}

public class AiServiceExceptionHandler : IExceptionHandler
{
    private readonly ILogger<AiServiceExceptionHandler> _logger;

    public AiServiceExceptionHandler(ILogger<AiServiceExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 1. Determine the ProblemDetails based on exception type
        var problemDetails = CreateProblemDetails(exception);

        // 2. Log the exception with structured data
        // We use LogError for server errors, but could adjust based on severity
        _logger.LogError(exception, "Handling exception: {Type} - {Title}", 
            problemDetails.Type, problemDetails.Title);

        // 3. Write the response
        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to signal that this exception was handled
        return true;
    }

    private ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            TimeoutException => new ProblemDetails
            {
                Type = "https://yourdomain.com/errors/timeout",
                Title = "Service Timeout",
                Status = 503,
                Detail = "The AI model processing request timed out."
            },
            ModelLoadException mle => new ProblemDetails
            {
                Type = "https://yourdomain.com/errors/model-load",
                Title = "Model Initialization Failed",
                Status = 500, // Internal server error as the service isn't ready
                Detail = mle.Message,
                Extensions = { ["model"] = mle.ModelName }
            },
            _ => new ProblemDetails
            {
                Type = "https://httpstatuses.com/500",
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An unexpected internal error occurred." // Sanitized message
            }
        };
    }
}
