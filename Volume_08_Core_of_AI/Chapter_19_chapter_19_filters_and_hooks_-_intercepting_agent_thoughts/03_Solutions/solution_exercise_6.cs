
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

using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Custom Attribute for Business Errors
[AttributeUsage(AttributeTargets.Class)]
public class BusinessErrorAttribute : Attribute { }

[BusinessError]
public class ValidationException : Exception
{
    public ValidationException(string msg) : base(msg) { }
}

// Global Error Filter (Intercepts and Normalizes)
public class GlobalErrorFilter : IKernelFilter
{
    public async Task InvokeAsync(KernelContext context, Func<KernelContext, Task> next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // Determine error type
            bool isBusinessError = ex.GetType().GetCustomAttributes(typeof(BusinessErrorAttribute), false).Length > 0;

            string message = isBusinessError 
                ? $"Business Error: {ex.Message}" 
                : "System Error: Service Unavailable";

            // Normalize to KernelResult
            // We create a JSON-like structure as requested
            string normalizedContent = $"{{ \"error\": \"true\", \"message\": \"{message}\" }}";

            // Update context result to stop propagation of the exception
            context.Result = KernelResult.FromValue(normalizedContent, context.Function);
        }
    }
}

// Global Error Hook (Logs to persistent store)
public class GlobalErrorHook : IKernelHook
{
    // Simulating a persistent store (static list for demo)
    public static List<string> ErrorLogs = new List<string>();

    public Task InvokeAsync(KernelContext context, Func<KernelContext, Task> next)
    {
        try
        {
            return next(context);
        }
        catch (Exception ex)
        {
            // Log the exception details
            ErrorLogs.Add($"[{DateTime.UtcNow}] {ex.GetType().Name}: {ex.Message}");
            
            // Re-throw to ensure the filter (which sits before this hook) can catch it if needed,
            // OR if we want the hook to be the last line of defense, we don't re-throw here.
            // Given the requirement says "Global Exception Handler", usually the Filter catches and normalizes.
            // However, if the Filter misses it, the Hook catches it.
            throw; 
        }
    }
}
