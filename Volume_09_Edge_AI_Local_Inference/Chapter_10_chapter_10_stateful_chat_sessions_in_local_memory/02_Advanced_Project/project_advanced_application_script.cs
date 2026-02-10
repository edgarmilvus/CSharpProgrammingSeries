
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace EdgeAI_LocalMemory
{
    // Real-world context: A localized "Personal Health Assistant" running entirely on a laptop.
    // It remembers your symptoms over a conversation session to provide better advice,
    // without sending private data to the cloud.
    class Program
    {
        // Configuration: Points to a local ONNX model file (e.g., Phi-3 Mini).
        // Note: This code assumes the model file exists. If not, the inference step will gracefully fail.
        private const string ModelPath = "phi-3-mini.onnx";

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: Local Health Assistant ===");
            Console.WriteLine("Running locally with ONNX Runtime. Session state is preserved in memory.");
            Console.WriteLine($"Looking for model at: {ModelPath}");

            if (!File.Exists(ModelPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[!] Model file not found. Please place 'phi-3-mini.onnx' in the execution directory.");
                Console.WriteLine("    The application will simulate the inference step for demonstration.");
                Console.ResetColor();
            }

            // Initialize the Stateful Session Manager
            var chatSession = new StatefulChatSession();

            // Main Interaction Loop
            while (true)
            {
                Console.Write("\n[User]: ");
                string userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput)) continue;
                if (userInput.ToLower() == "exit") break;

                // 1. Process input and update memory
                chatSession.AddMessage("User", userInput);

                // 2. Generate response using local model
                Console.Write("[Assistant]: ");
                string response = await chatSession.GenerateResponseAsync(ModelPath);

                // 3. Display response
                Console.WriteLine(response);

                // 4. Store assistant response in memory for context
                chatSession.AddMessage("Assistant", response);
            }
        }
    }

    /// <summary>
    /// Manages the conversation history and token limits.
    /// This is the core "Stateful" component of the application.
    /// </summary>
    public class StatefulChatSession
    {
        // Using a List to simulate a dynamic conversation history buffer.
        // In a real high-performance scenario, we might use a circular buffer or a linked list.
        private readonly List<Message> _history = new List<Message>();

        // Hardware constraint simulation: Max tokens to keep in context.
        // Phi-3 Mini context window is 128k, but we limit this to 2048 for local RAM safety.
        private const int MaxContextTokens = 2048;
        private const int ApproxCharsPerToken = 4; // Rough estimation for token counting

        public void AddMessage(string role, string content)
        {
            var msg = new Message { Role = role, Content = content };
            _history.Add(msg);
            PruneHistory(); // Critical: Manage memory constraints immediately
        }

        /// <summary>
        /// Prunes the conversation history if it exceeds the token limit.
        /// Strategy: Remove the oldest exchanges (FIFO) while keeping the system prompt.
        /// </summary>
        private void PruneHistory()
        {
            int currentTokenCount = 0;
            
            // Calculate tokens (approximation based on char count)
            foreach (var msg in _history)
            {
                currentTokenCount += msg.Content.Length / ApproxCharsPerToken;
            }

            // If we are over the limit, remove the oldest messages
            while (currentTokenCount > MaxContextTokens && _history.Count > 0)
            {
                var removedMsg = _history[0];
                currentTokenCount -= removedMsg.Content.Length / ApproxCharsPerToken;
                _history.RemoveAt(0);
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[System]: Pruned context history to maintain hardware limits. Removed: {removedMsg.Content.Substring(0, Math.Min(20, removedMsg.Content.Length))}...");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Formats the history into a prompt string for the ONNX model.
        /// </summary>
        private string BuildPrompt()
        {
            var sb = new StringBuilder();
            sb.Append("<|system|>You are a helpful local health assistant. Provide concise advice based on symptoms.<|end|>");
            
            foreach (var msg in _history)
            {
                sb.Append($"<|{msg.Role.ToLower()}|>{msg.Content}<|end|>");
            }
            
            sb.Append("<|assistant|>"); // Prepare for generation
            return sb.ToString();
        }

        /// <summary>
        /// Connects to ONNX Runtime to perform local inference.
        /// </summary>
        public async Task<string> GenerateResponseAsync(string modelPath)
        {
            // If model doesn't exist, simulate a response for the code example to run.
            if (!File.Exists(modelPath))
            {
                await Task.Delay(500); // Simulate processing time
                return "[Simulated Local Inference]: Based on your history, I recommend resting and staying hydrated.";
            }

            return await Task.Run(() =>
            {
                try
                {
                    // 1. Load the ONNX Model
                    using var session = new InferenceSession(modelPath);
                    
                    // 2. Tokenize Input (Simplified for this example)
                    // In production, you would use a specific Tokenizer library (e.g., Microsoft.ML.Tokenizers)
                    var inputString = BuildPrompt();
                    var inputTensor = Tokenize(inputString); 

                    // 3. Create Inputs for ONNX Runtime
                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
                    };

                    // 4. Run Inference
                    using var results = session.Run(inputs);
                    
                    // 5. Decode Output (Simplified)
                    var outputTensor = results[0].AsTensor<long>();
                    string decodedResponse = Detokenize(outputTensor);
                    
                    return decodedResponse;
                }
                catch (Exception ex)
                {
                    return $"[Error running ONNX model: {ex.Message}]";
                }
            });
        }

        // --- Simplified Tokenizer / Detokenizer for Demonstration ---
        // NOTE: Real ONNX LLMs require complex BPE/WordPiece tokenization. 
        // This is a placeholder to demonstrate the data flow.
        private Tensor<long> Tokenize(string text)
        {
            // In a real app, this converts text to numeric IDs.
            // Here we create a dummy tensor of shape [1, sequence_length]
            var data = new long[1, 50]; 
            for(int i=0; i<50 && i<text.Length; i++) 
                data[0,i] = (long)text[i]; // Naive char-to-int mapping
            return new DenseTensor<long>(data, new int[] { 1, 50 });
        }

        private string Detokenize(Tensor<long> tensor)
        {
            // In a real app, this converts numeric IDs back to text.
            var sb = new StringBuilder();
            foreach (var val in tensor)
            {
                if (val > 32 && val < 126) sb.Append((char)val);
            }
            return sb.ToString();
        }
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
