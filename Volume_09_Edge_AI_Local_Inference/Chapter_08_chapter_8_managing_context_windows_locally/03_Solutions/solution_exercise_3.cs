
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
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using LocalLLM.Managers; // Assuming previous exercises are in this namespace

namespace LocalLLM.Services
{
    public class InferenceOptions
    {
        public int MaxTokens { get; set; } = 512;
        public float Temperature { get; set; } = 0.7f;
    }

    // 1. Stateful Session & 5. Code Structure
    public class OnnxInferenceService : IDisposable
    {
        private readonly InferenceSession _session;
        private readonly ContextWindowManager _contextManager; // Dependency Injection
        private readonly TokenEstimator _tokenEstimator;
        
        // Persistent conversation state
        private List<string> _conversationHistory = new List<string>();

        public OnnxInferenceService(string modelPath, ContextWindowManager contextManager)
        {
            var sessionOptions = new SessionOptions();
            // Attempt to use GPU if available, otherwise CPU
            sessionOptions.AppendExecutionProviderDml(0); // DirectML for Windows GPU
            sessionOptions.AppendExecutionProviderCpu();
            
            _session = new InferenceSession(modelPath, sessionOptions);
            _contextManager = contextManager;
            _tokenEstimator = new TokenEstimator();
        }

        // 2. Input Processing Pipeline
        public async Task<string> GenerateResponseAsync(string userMessage, InferenceOptions options)
        {
            // Step 1: Pass raw input to Context Manager
            // The manager handles token counting and history pruning
            var (systemPrompt, optimizedHistory) = _contextManager.GetOptimizedContext(userMessage);
            
            // Construct the full prompt
            var promptBuilder = new System.Text.StringBuilder();
            promptBuilder.AppendLine(systemPrompt);
            foreach (var msg in optimizedHistory)
            {
                promptBuilder.AppendLine(msg);
            }
            promptBuilder.AppendLine($"User: {userMessage}");
            promptBuilder.Append("Assistant: "); // The prompt must end with the assistant prefix

            string finalPrompt = promptBuilder.ToString();

            // Step 2: Tokenize (Abstracted for brevity)
            // In a real implementation, you would use the Tokenizer here to get input_ids
            var inputTensor = Tokenize(finalPrompt); 

            // Prepare ONNX inputs
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                // ... other inputs like attention_mask, position_ids
            };

            // 3. Memory Constraint Handling
            try
            {
                // Run inference
                using (var outputs = _session.Run(inputs))
                {
                    // Process outputs (e.g., get logits, decode to text)
                    var response = DecodeOutput(outputs); // Abstracted method
                    
                    // 4. Output Handling: Append to history
                    // We append the full user message and the generated response
                    _contextManager.AddToHistory($"User: {userMessage}");
                    _contextManager.AddToHistory($"Assistant: {response}");

                    return response;
                }
            }
            catch (OutOfMemoryException)
            {
                // Fallback: Aggressive Pruning
                Console.WriteLine("Warning: Out of Memory. Triggering aggressive pruning.");
                
                // Strategy: Switch to Minimal prompt and clear half of history
                // Note: In a real app, we would modify the ContextManager to force a specific variant
                // Here we simulate clearing history to free up space immediately.
                var historyCount = _contextManager.GetHistoryCount(); 
                _contextManager.RemoveOldest(historyCount / 2); 

                // Retry once with the smaller context
                // (In a real scenario, we might return a specific message or retry with lower max_tokens)
                return await GenerateResponseAsync(userMessage, options); 
            }
            catch (Exception ex)
            {
                // Handle other ONNX errors
                return $"[System Error: {ex.Message}]";
            }
        }

        // Helper methods (Abstracted for the exercise context)
        private Tensor<int> Tokenize(string prompt)
        {
            // Mock tokenization logic
            return new DenseTensor<int>(new int[] { 1, 10 }, 2); 
        }

        private string DecodeOutput(OrtValue outputs)
        {
            // Mock decoding logic
            return "This is a generated response.";
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
