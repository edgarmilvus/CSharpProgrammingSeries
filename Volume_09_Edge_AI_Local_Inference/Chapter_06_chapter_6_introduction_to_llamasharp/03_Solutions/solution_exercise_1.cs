
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

using System;
using System.Collections.Generic;
using System.Linq;
using LlamaCppLib;

namespace LlamaSharpExercises
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Configuration - Replace with your actual GGUF model path
            string modelPath = @"C:\Models\phi-2-q4.gguf"; 
            
            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"Model file not found at: {modelPath}");
                Console.WriteLine("Please update the modelPath variable in the code.");
                return;
            }

            Console.WriteLine("Initializing Model...");
            
            // 1. Load Model
            using var model = LoadModel(modelPath);
            
            // 2. Initialize Context
            using var context = InitializeContext(model);

            // 3. Start Conversation Loop
            RunConversationLoop(context);
        }

        static LlamaModel LoadModel(string modelPath)
        {
            var params_ = new LlamaModelParams
            {
                // Set threads to logical processor count
                Threads = (uint)Environment.ProcessorCount,
                // CPU-only inference
                GpuLayers = 0 
            };

            // LlamaSharp typically loads the model via the constructor
            return new LlamaModel(modelPath, params_);
        }

        static LlamaContext InitializeContext(LlamaModel model)
        {
            var ctxParams = new LlamaContextParams
            {
                ContextSize = 2048,
                BatchSize = 512
            };

            return model.CreateContext(ctxParams);
        }

        static void RunConversationLoop(LlamaContext context)
        {
            // Initial prompt
            string prompt = "The meaning of life is";
            List<int> conversationHistoryTokens = new List<int>();

            while (true)
            {
                Console.WriteLine($"\n[User]: {prompt}");

                // Tokenize the prompt
                var tokens = TokenizePrompt(context, prompt);
                
                // Append to history (for context window management logic later, 
                // though for this simple exercise we just re-process the whole conversation)
                conversationHistoryTokens.AddRange(tokens);

                // Initialize context with the full history
                // Note: In a real scenario, we might use context.LoadSession or reset state.
                // Here we simulate by feeding the history tokens.
                context.Reset(); // Clear previous state
                context.Eval(tokens.ToArray()); // Evaluate the prompt tokens

                Console.Write("[Assistant]: ");
                
                // Inference Loop
                int maxTokens = 50;
                int tokensGenerated = 0;
                
                while (tokensGenerated < maxTokens)
                {
                    // Get the next token
                    var nextToken = context.GetNextToken();

                    // Check for end of sequence (assuming EOS token ID is 2, common in many models)
                    // Note: Token IDs vary by model. For robustness, check model.Vocab.EosTokenId
                    if (nextToken == context.Model.Vocab.EosTokenId) 
                        break;

                    // Decode and print
                    string piece = context.Model.Vocab.TokenToString(nextToken);
                    Console.Write(piece);
                    Console.Out.Flush(); // Ensure immediate output

                    // Update context state with the new token
                    context.AcceptToken(nextToken);

                    tokensGenerated++;
                }

                Console.WriteLine(); // New line after generation

                // Interactive Challenge: User Input
                Console.WriteLine("\n[Type 'exit' to quit, or enter a follow-up message]");
                Console.Write("> ");
                string? userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit")
                {
                    break;
                }

                // Prepare for next iteration
                // In this simple loop, we treat the user input as the new prompt.
                // To maintain history, we would append it to a conversation string builder.
                // For this exercise, we simply update the prompt variable.
                prompt = userInput ?? "";
            }
        }

        static int[] TokenizePrompt(LlamaContext context, string prompt)
        {
            // false = do not add BOS/EOS token automatically for this specific logic
            return context.Model.Tokenize(prompt, false);
        }
    }

    // Mock classes to represent the LlamaSharp API structure for compilation context
    // In a real project, you would reference the actual LlamaSharp NuGet package.
    namespace LlamaCppLib
    {
        public class LlamaModelParams
        {
            public uint Threads { get; set; }
            public int GpuLayers { get; set; }
        }

        public class LlamaContextParams
        {
            public int ContextSize { get; set; }
            public int BatchSize { get; set; }
        }

        public class Vocab
        {
            public int EosTokenId => 2; // Example ID
            public string TokenToString(int token) => "token"; // Mock
        }

        public class LlamaModel : IDisposable
        {
            public Vocab Vocab { get; } = new Vocab();
            
            public LlamaModel(string path, LlamaModelParams p) { /* Load logic */ }
            public LlamaContext CreateContext(LlamaContextParams p) => new LlamaContext(this);
            public int[] Tokenize(string text, bool addBos) => new int[] { 1, 2, 3 }; // Mock
            public void Dispose() { /* Cleanup */ }
        }

        public class LlamaContext : IDisposable
        {
            private LlamaModel _model;
            public LlamaModel Model => _model;

            public LlamaContext(LlamaModel model) { _model = model; }

            public void Reset() { /* Clear KV cache */ }
            public void Eval(int[] tokens) { /* Run inference */ }
            public int GetNextToken() => new Random().Next(100); // Mock sampling
            public void AcceptToken(int token) { /* Update state */ }
            public void Dispose() { /* Cleanup */ }
        }
    }
}
