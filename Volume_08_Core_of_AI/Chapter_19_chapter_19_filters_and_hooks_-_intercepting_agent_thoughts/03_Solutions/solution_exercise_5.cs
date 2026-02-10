
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

using Microsoft.SemanticKernel;
using System;
using System.Threading.Tasks;

public class ConditionalApprovalFilter : IKernelFilter
{
    private readonly Kernel _kernel;

    public ConditionalApprovalFilter(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task InvokeAsync(KernelContext context, Func<KernelContext, Task> next)
    {
        // 1. Check for bypass flag (Edge Case: Infinite Loop)
        if (context.Items.TryGetValue("IsInternalCall", out var isInternal) && (bool)isInternal)
        {
            await next(context);
            return;
        }

        // 2. Execute the main pipeline
        await next(context);

        // 3. Inspect Result (Post-Execution)
        if (context.Result?.Value?.ToString() is string resultString && resultString.Contains("APPROVED"))
        {
            // 4. Trigger Secondary Function
            // We must mark the context to prevent infinite recursion if LogApproval triggers this filter again
            context.Items["IsInternalCall"] = true;

            // Create a new context or reuse existing arguments for the secondary call
            var secondaryArgs = new KernelFunctionArguments(context.Arguments);
            
            // Invoke the mock function
            // Note: In a real scenario, we might use _kernel.InvokeAsync directly
            // but we need to simulate the pipeline execution here.
            var logFunction = _kernel.Plugins["ApprovalPlugin"]["LogApproval"];
            await _kernel.InvokeAsync(logFunction, secondaryArgs);
        }
    }
}

// Mock for the LogApproval function logic
public class ApprovalPlugin
{
    [KernelFunction]
    public string LogApproval(KernelContext context)
    {
        // Logic to log approval
        return "Approval Logged";
    }
}
