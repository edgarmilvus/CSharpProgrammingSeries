
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntimeGenAI;

public class InteractiveChat
{
    public static void Main(string[] args)
    {
        string modelPath = "./models/phi-3-mini"; // Example path
        var history = new List<string>();
        string systemPrompt = "You are a helpful assistant on an edge device.";

        try
        {
            // Load model once and keep it alive for the session
            using var model = new OnnxGenAIModel(new ModelOptions { ModelPath = modelPath });
            using var tokenizer = new OnnxGenAITokenizer(model);

            Console.WriteLine("Chat initialized. Type 'exit' to quit.");

            while (true)
            {
                Console.Write("\nUser: ");
                string userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit") break;

                // Construct the prompt with history
                // Note: A real implementation would use a chat template (e.g., ChatML format)
                string currentPrompt = $"{systemPrompt}. History: {string.Join(" ", history)}. User: {userInput}. Assistant:";
                
                // 1. Tokenize
                using var inputTensors = tokenizer.Encode(new List<string> { currentPrompt });
                
                // 2. Create Sequences object for generation state
                using var sequences = new OnnxGenAISequences(inputTensors);

                Console.Write("Assistant: ");

                // 3. Manual Generation Loop (Token by Token)
                // We use a loop instead of model.Generate() to print tokens as they arrive.
                while (!sequences.IsStopped)
                {
                    // Generate the next token ID
                    model.GenerateNextToken(sequences);

                    // Get the last generated token
                    // Note: Depending on library version, we might need to access the sequence data
                    // For this exercise, we assume we can decode the sequence to see the new text.
                    // A more optimized approach would be to decode only the last token.
                    
                    // Decode the entire sequence to get the full text (simplified for exercise)
                    // In a high-performance loop, we would decode only the new token ID.
                    string decodedText = tokenizer.Decode(sequences);
                    
                    // To avoid printing the whole prompt every time, we track length or 
                    // simply print the new character (this requires careful state management).
                    // For this exercise, we will print the full decoded text and clear line 
                    // to simulate streaming, or simply print tokens.
                    
                    // A simplified "streaming" simulation:
                    Console.Write("."); // Placeholder for actual token streaming logic
                }

                // Final decode for the complete response
                string fullResponse = tokenizer.Decode(sequences);
                Console.WriteLine($"\nAssistant: {fullResponse}");

                // Update history
                history.Add($"User: {userInput}");
                history.Add($"Assistant: {fullResponse}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
