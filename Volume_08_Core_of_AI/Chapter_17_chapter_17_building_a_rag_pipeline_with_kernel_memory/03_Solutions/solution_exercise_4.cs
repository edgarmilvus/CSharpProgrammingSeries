
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace KernelMemory.Exercises;

public class MultiAgentRAGSystem
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public MultiAgentRAGSystem(Kernel kernel, IChatCompletionService chatService)
    {
        _kernel = kernel;
        _chatService = chatService;
    }

    public async Task<string> ExecuteWorkflowAsync(string userQuery)
    {
        Console.WriteLine($"[Router] Received query: {userQuery}");

        // 1. Define Agents
        var researcherAgent = new ChatCompletionAgent()
        {
            Name = "Researcher",
            Instructions = "You retrieve technical details from the 'Technical Manuals' collection. Provide factual data.",
            Kernel = _kernel,
            Arguments = new KernelArguments() { ["collection"] = "TechManuals" }
        };

        var validatorAgent = new ChatCompletionAgent()
        {
            Name = "Validator",
            Instructions = "You retrieve 'Compliance Standards'. Check if the provided answer complies. Return 'Compliant' or 'Non-Compliant' with reason.",
            Kernel = _kernel,
            Arguments = new KernelArguments() { ["collection"] = "Compliance" }
        };

        var correctionAgent = new ChatCompletionAgent()
        {
            Name = "Correction",
            Instructions = "You rewrite answers to strictly adhere to compliance standards.",
            Kernel = _kernel
        };

        // 2. Parallel Execution (Agentic Pattern: Parallel Chaining)
        Console.WriteLine("[Router] Dispatching to Researcher and Validator in parallel...");
        
        var researcherTask = researcherAgent.InvokeAsync(userQuery);
        var validatorTask = validatorAgent.InvokeAsync(userQuery); // Validator checks the raw query or context

        await Task.WhenAll(researcherTask, validatorTask);

        var researcherResponse = researcherTask.Result.Content;
        var validatorResponse = validatorTask.Result.Content;

        Console.WriteLine($"[Researcher] Output: {researcherResponse}");
        Console.WriteLine($"[Validator] Assessment: {validatorResponse}");

        // 3. Router Synthesis & Conditional Routing
        bool isCompliant = validatorResponse.Contains("Compliant", StringComparison.OrdinalIgnoreCase);

        if (isCompliant)
        {
            return researcherResponse;
        }
        else
        {
            Console.WriteLine("[Router] Non-Compliant detected. Triggering Correction Agent...");
            
            // 4. Correction Flow
            // We pass the original query, the non-compliant answer, and the compliance violation.
            var correctionContext = $"Original Query: {userQuery}\nResearcher Answer: {researcherResponse}\nCompliance Issue: {validatorResponse}";
            
            var correctionResult = await correctionAgent.InvokeAsync(correctionContext);
            return correctionResult.Content;
        }
    }
}
