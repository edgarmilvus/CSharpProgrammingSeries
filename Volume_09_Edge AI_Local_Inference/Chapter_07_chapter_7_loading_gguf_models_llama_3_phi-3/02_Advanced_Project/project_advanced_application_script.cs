
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace EdgeAI_LocalInference
{
    /// <summary>
    /// Real-World Problem: Smart Home Voice Assistant for Elderly Care
    /// Scenario: An edge device (e.g., Raspberry Pi) acts as a voice assistant for an elderly person.
    /// It needs to interpret natural language commands to control devices (lights, thermostat) and
    /// respond with empathetic, context-aware feedback without sending sensitive data to the cloud.
    /// 
    /// Solution: We use a quantized Phi-3 Mini GGUF model loaded via ONNX Runtime GenAI.
    /// The app simulates a continuous conversation loop, processing user input and generating
    /// local inference responses to trigger smart home actions.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: Elderly Care Assistant (Local Inference) ===");
            Console.WriteLine("Initializing ONNX Runtime GenAI with Phi-3 GGUF model...");

            // 1. Configuration & Path Management
            // In a real deployment, these paths are dynamic or config-based.
            // We check if the model directory exists to handle edge cases gracefully.
            string modelDir = Path.Combine(Directory.GetCurrentDirectory(), "phi-3-mini-gguf-onnx");
            if (!Directory.Exists(modelDir))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Model directory not found at '{modelDir}'.");
                Console.WriteLine("Please ensure the ONNX converted GGUF model is present.");
                Console.ResetColor();
                return;
            }

            // 2. Model Initialization
            // We wrap the initialization in a try-catch block to handle native library loading errors
            // or missing model files, which is common in edge device environments.
            Model? model = null;
            Tokenizer? tokenizer = null;
            try
            {
                // Load the ONNX model. ONNX Runtime GenAI handles the GGUF weights internally
                // if the directory contains the correct ONNX files and configuration.
                model = new Model(modelDir);
                
                // Initialize the tokenizer associated with the model.
                // This converts text to tokens and vice versa.
                tokenizer = new Tokenizer(model);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Model loaded successfully.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Critical Initialization Error: {ex.Message}");
                Console.ResetColor();
                return;
            }

            // 3. The Application Loop (Inference Engine)
            // We simulate a continuous listening state. In a real app, this would be
            // hooked to a microphone input event.
            RunConversationLoop(model, tokenizer);

            // 4. Cleanup
            // Explicit disposal is crucial in C# when using unmanaged resources (like ONNX native pointers).
            tokenizer?.Dispose();
            model?.Dispose();
            Console.WriteLine("Session ended. Resources released.");
        }

        /// <summary>
        /// Handles the core logic of processing user input and generating responses.
        /// </summary>
        static void RunConversationLoop(Model model, Tokenizer tokenizer)
        {
            // System Prompt: Defines the AI's persona and constraints.
            // This is critical for consistent behavior in local models.
            string systemPrompt = "You are a helpful, empathetic assistant for elderly care. " +
                                  "Keep responses short and clear. " +
                                  "If a command to control a device is detected, respond with the action taken.";

            // Conversation History: Local LLMs benefit from context, but we must manage memory.
            // We use a simple List to store previous interactions.
            List<string> conversationHistory = new List<string>();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nUser: ");
                string? userInput = Console.ReadLine();

                // Exit condition
                if (userInput?.ToLower() == "exit" || userInput?.ToLower() == "quit")
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                // 5. Prompt Engineering
                // Constructing the prompt manually to guide the model.
                // We inject the system prompt and conversation history.
                string fullPrompt = $"System: {systemPrompt}\n";
                
                // Add history (limited to last 4 exchanges to prevent context overflow on edge devices)
                int historyStartIndex = Math.Max(0, conversationHistory.Count - 8);
                for (int i = historyStartIndex; i < conversationHistory.Count; i++)
                {
                    fullPrompt += conversationHistory[i] + "\n";
                }
                
                fullPrompt += $"User: {userInput}\nAssistant:";

                // 6. Tokenization
                // Convert the string prompt into numerical tokens the model understands.
                // In ONNX GenAI, this is handled by the Tokenizer class.
                Console.WriteLine("Processing input...");
                var inputTokens = tokenizer.Encode(fullPrompt);

                // 7. Generator Parameters
                // Configuring inference parameters (Temperature, Max Length).
                // Lower temperature (0.0 - 0.7) makes the model deterministic, preferred for assistants.
                var generatorParams = new GeneratorParams(model);
                generatorParams.SetInputSequences(inputTokens);
                generatorParams.SetSearchOption("max_length", 150); // Limit response length
                generatorParams.SetSearchOption("temperature", 0.7); // Creativity level
                generatorParams.SetSearchOption("top_k", 50); // Top K sampling
                generatorParams.SetSearchOption("top_p", 0.9); // Top P sampling

                // 8. Inference Execution
                // The Generator creates the sequence of tokens.
                // We wrap this in a try-catch to handle potential out-of-memory errors on low-end edge devices.
                string response = "";
                try
                {
                    using var generator = new Generator(model, generatorParams);
                    
                    // 9. Token Generation Loop
                    // We generate tokens one by one to simulate streaming (crucial for user experience).
                    // While the model has more tokens to generate, compute the next token.
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Assistant: ");
                    
                    while (!generator.IsDone())
                    {
                        // Compute the next token
                        generator.ComputeLogits();
                        generator.GenerateNextToken();
                        
                        // Decode the new token to text
                        // Note: In optimized scenarios, we might decode in batches, 
                        // but for simple console apps, single token decoding is fine.
                        var newTokens = generator.GetSequence(0);
                        var lastToken = new int[] { newTokens[newTokens.Length - 1] };
                        string tokenText = tokenizer.Decode(lastToken);
                        
                        Console.Write(tokenText);
                        response += tokenText;
                    }
                    Console.WriteLine(); // New line after stream finishes
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Inference Error: {ex.Message}");
                    Console.ResetColor();
                    continue;
                }

                // 10. Post-Processing (Function Calling Simulation)
                // A real edge AI app often needs to trigger hardware actions.
                // We simulate a simple rule-based parser to detect keywords in the response.
                ProcessDeviceCommands(response);

                // 11. Update History
                // Store the interaction for the next loop iteration.
                conversationHistory.Add($"User: {userInput}");
                conversationHistory.Add($"Assistant: {response}");
            }
        }

        /// <summary>
        /// Simulates a function-calling capability by parsing the text response for specific keywords.
        /// This bridges the gap between LLM output and physical device control.
        /// </summary>
        static void ProcessDeviceCommands(string response)
        {
            // Simple string matching for demonstration. 
            // In production, you would use structured output (JSON) from the LLM.
            string lowerResponse = response.ToLower();

            if (lowerResponse.Contains("light on") || lowerResponse.Contains("turn on light"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("[SYSTEM ACTION]: Turning Living Room Lights ON.");
                Console.ResetColor();
            }
            else if (lowerResponse.Contains("light off") || lowerResponse.Contains("turn off light"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("[SYSTEM ACTION]: Turning Living Room Lights OFF.");
                Console.ResetColor();
            }
            else if (lowerResponse.Contains("temperature") || lowerResponse.Contains("warm"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("[SYSTEM ACTION]: Adjusting Thermostat to 22°C.");
                Console.ResetColor();
            }
        }
    }
}
