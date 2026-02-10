
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

// Source File: theory_theoretical_foundations_part5.cs
// Description: Theoretical Foundations
// ==========================================

using System;

// The Blueprint
class AIAgent
{
    // Fields (Data stored on the Heap)
    private string agentName;
    private string systemPrompt;
    private int tokensUsed; // Tracks usage (like a cost counter)

    // Constructor
    public AIAgent(string name, string prompt)
    {
        agentName = name;
        systemPrompt = prompt;
        tokensUsed = 0;
    }

    // Method: Simulates generating a response
    // Returns a string (Chapter 14: Return Values)
    public string GenerateResponse(string userPrompt)
    {
        // Calculate cost (simulated logic)
        int promptLength = userPrompt.Length;
        tokensUsed = tokensUsed + promptLength; // Increment (Chapter 4)

        // Construct the response using String Interpolation (Chapter 3)
        string fullResponse = $"[{agentName}]: Processed your request. Context: {systemPrompt}";
        
        return fullResponse;
    }

    // Method: Check usage stats
    public void LogUsage()
    {
        Console.WriteLine($"Agent {agentName} has used {tokensUsed} tokens.");
    }
}

// Main Program to use the class
class Program
{
    static void Main()
    {
        // 1. Instantiate the AI Agent
        // We create a specific instance named 'ChatBot'
        AIAgent chatBot = new AIAgent("ChatBot", "You are a helpful assistant.");

        // 2. Interact with the object
        // We call methods on the specific instance
        string response1 = chatBot.GenerateResponse("Hello!");
        Console.WriteLine(response1);

        string response2 = chatBot.GenerateResponse("What is the weather?");
        Console.WriteLine(response2);

        // 3. Check the state (Encapsulation in action)
        // The 'tokensUsed' field is private, so we use the public method to see it
        chatBot.LogUsage();
    }
}
