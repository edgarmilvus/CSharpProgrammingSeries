
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.SemanticKernel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

// 1. Define the custom exception
public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
}

// 2. Implement the IKernelFilter
public class InputValidationFilter : IKernelFilter
{
    private readonly HashSet<string> _restrictedKeywords;

    public InputValidationFilter(IEnumerable<string> restrictedKeywords)
    {
        _restrictedKeywords = new HashSet<string>(restrictedKeywords, StringComparer.OrdinalIgnoreCase);
    }

    public async Task InvokeAsync(KernelContext context, Func<KernelContext, Task> next)
    {
        // 3. Extract the Input argument safely
        // Note: KernelFunctionArguments inherits from IDictionary<string, object?>
        if (context.Arguments.TryGetValue("input", out var inputValue) && inputValue != null)
        {
            string inputText = inputValue.ToString();

            // 4. Check for restricted keywords
            // Using LINQ for efficient checking
            var detectedKeyword = _restrictedKeywords.FirstOrDefault(k => inputText.Contains(k, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(detectedKeyword))
            {
                // 5. Halt execution immediately
                throw new SecurityException($"Security violation: Restricted keyword '{detectedKeyword}' detected.");
            }
        }
        // 6. Edge Case: If Input is missing or null, we simply proceed (pass-through).

        // 7. Proceed to the next filter or kernel function
        await next(context);
    }
}

// --- Usage Example (Not required for unit test, but for context) ---
/*
var kernel = new KernelBuilder()
    .WithServices(s => s.AddOpenAIChatCompletion("model-id", "api-key"))
    .WithFilters(f => f.AddInputValidation(new[] { "password", "ssn", "credit card" }))
    .Build();
*/
