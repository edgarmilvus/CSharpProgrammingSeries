
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

// Source File: solution_exercise_11.cs
// Description: Solution for Exercise 11
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunHumanInTheLoopAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var safetyAgent = new ChatCompletionAgent(kernel, "Analyze code. Does it contain sensitive operations (File I/O, Database)? Answer Yes or No.");
    var reviewAgent = new ChatCompletionAgent(kernel, "You are a Code Review Agent. Review the code for style.");

    // Simulated Code Input
    string code = "File.WriteAllText(\"log.txt\", \"data\");"; // Sensitive

    // 1. Safety Check
    var safetyResponse = await safetyAgent.InvokeAsync(code);
    Console.WriteLine($"Safety Check: {safetyResponse}");

    if (safetyResponse.ToString().Trim().StartsWith("Yes"))
    {
        // 2. Interrupt State
        Console.Write("\n[SECURITY ALERT] Sensitive operation detected. Type 'APPROVE' to continue: ");
        string approval = Console.ReadLine() ?? "";

        if (approval.Equals("APPROVE", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Human approved. Proceeding to review...");
            var review = await reviewAgent.InvokeAsync(code);
            Console.WriteLine($"Review Result: {review}");
        }
        else
        {
            Console.WriteLine("Human denied. Workflow terminated.");
        }
    }
    else
    {
        var review = await reviewAgent.InvokeAsync(code);
        Console.WriteLine($"Review Result: {review}");
    }
}
