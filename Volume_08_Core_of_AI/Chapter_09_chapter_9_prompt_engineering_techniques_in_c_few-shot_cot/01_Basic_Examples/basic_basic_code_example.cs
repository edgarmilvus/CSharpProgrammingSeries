
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. SETUP: Initialize the Kernel with a chat completion service.
        // In a real scenario, configure this with your Azure OpenAI or OpenAI API key.
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o-mini", // Or any available model
                apiKey: "fake-api-key-for-demo") 
            .Build();

        // 2. FEW-SHOT PROMPT CONSTRUCTION:
        // We manually construct a prompt that includes "shots" (examples).
        // This guides the model on the expected input/output format.
        string fewShotPrompt = """
            You are a helpful assistant that categorizes support tickets.
            Classify the following ticket into one of these categories: 
            [Billing, Technical Support, General Inquiry].

            Examples (Few-Shots):
            Ticket: "I can't login to my account."
            Category: Technical Support

            Ticket: "Where is my invoice for last month?"
            Category: Billing

            Ticket: "How do I reset my password?"
            Category: Technical Support

            Ticket: "I have a feature request."
            Category: General Inquiry

            Now classify this new ticket:
            Ticket: "{{ $input }}"
            Category:
            """;

        // 3. AGENT CREATION:
        // Create a chat completion agent using the kernel and the custom prompt.
        // We use the ChatCompletionAgent class which leverages the IChatCompletionService.
        var agent = new ChatCompletionAgent(
            kernel: kernel,
            instructions: fewShotPrompt);

        // 4. EXECUTION:
        // Simulate a user query and pass it to the agent.
        // We use the ChatHistory object to maintain context (though here it's a single turn).
        string userTicket = "My internet connection keeps dropping.";
        
        Console.WriteLine($"Input Ticket: \"{userTicket}\"");
        Console.WriteLine("Processing with Few-Shot Agent...\n");

        // The agent invokes the LLM with the constructed prompt.
        var response = await agent.InvokeAsync(userTicket);

        // 5. OUTPUT:
        // The response should ideally be "Technical Support" based on the pattern 
        // established in the examples (login issues, password resets).
        Console.WriteLine($"Predicted Category: {response.Content}");
    }
}
