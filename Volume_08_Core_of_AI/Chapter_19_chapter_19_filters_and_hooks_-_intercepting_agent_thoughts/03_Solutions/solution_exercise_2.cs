
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

using Microsoft.SemanticKernel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

// Mock Logger interface for demonstration
public interface IMetricLogger
{
    void LogMetric(string name, double value, string? additionalInfo = null);
}

public class ExecutionTimeHook : IKernelHook
{
    private readonly IMetricLogger _logger;

    public ExecutionTimeHook(IMetricLogger logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(KernelContext context, Func<KernelContext, Task> next)
    {
        // 1. Capture start time
        var sw = Stopwatch.StartNew();
        Exception? caughtException = null;

        try
        {
            // 2. Allow function to execute (passing to next hook/filter)
            await next(context);
        }
        catch (Exception ex)
        {
            // 5. Capture exception but don't swallow it yet
            caughtException = ex;
        }
        finally
        {
            sw.Stop();
            
            // 3 & 4. Log duration and additional metrics
            var duration = sw.Elapsed.TotalMilliseconds;
            var funcName = context.Function?.Name ?? "UnknownFunction";
            
            // Simulate token count calculation (Advanced Requirement)
            string? resultContent = context.Result?.Value?.ToString();
            int tokenCount = !string.IsNullOrEmpty(resultContent) ? resultContent.Length : 0;

            if (caughtException != null)
            {
                // Log failure duration
                _logger.LogMetric(funcName, duration, $"Failed; Tokens: 0");
                // Re-throw the original exception
                throw caughtException;
            }
            else
            {
                // Log success duration
                _logger.LogMetric(funcName, duration, $"Success; Tokens: {tokenCount}");
            }
        }
    }
}
