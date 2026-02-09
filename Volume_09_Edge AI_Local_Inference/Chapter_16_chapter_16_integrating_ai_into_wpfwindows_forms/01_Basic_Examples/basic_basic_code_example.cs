
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalPhi2Inference
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Configuration
            // In a real app, this path would be relative to the executable.
            // Ensure you have the 'phi-2' ONNX model file downloaded locally.
            // Model source: https://huggingface.co/microsoft/phi-2/resolve/main/onnx/model.onnx
            string modelPath = @"C:\Models\phi-2\model.onnx"; 
            
            // 2. Define the prompt
            string userPrompt = "Write a haiku about debugging code.";
            
            Console.WriteLine($"Loading model from: {modelPath}");
            Console.WriteLine($"Prompt: {userPrompt}\n");

            try
            {
                // 3. Execute Inference
                string generatedText = await GenerateTextAsync(modelPath, userPrompt);
                
                // 4. Output Result
                Console.WriteLine("Generated Output:");
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine(generatedText);
                Console.WriteLine("------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Ensure the model path is correct and the ONNX Runtime is installed.");
            }
        }

        /// <summary>
        /// Runs the ONNX model asynchronously to generate text.
        /// </summary>
        static async Task<string> GenerateTextAsync(string modelPath, string prompt)
        {
            return await Task.Run(() =>
            {
                // Load the ONNX model using the session options.
                // We enable execution providers (GPU) if available.
                var sessionOptions = new SessionOptions();
                sessionOptions.AppendExecutionProvider_DML(0); // DirectML for Windows GPU support
                sessionOptions.AppendExecutionProvider_CPU();   // Fallback to CPU
                
                using var session = new InferenceSession(modelPath, sessionOptions);

                // --- Tokenization Simulation ---
                // In a production environment, you would use the 'Microsoft.ML.OnnxRuntime.Transformers' 
                // library or a dedicated tokenizer library. 
                // For this "Hello World" example, we simulate a tokenizer by mapping characters 
                // to integer IDs. Phi-2 uses the GPT2 tokenizer, which is complex.
                // We will use a simplified mock tokenizer for demonstration.
                var tokenizer = new SimpleMockTokenizer();
                var inputIds = tokenizer.Encode(prompt);

                // --- Prepare Input Tensors ---
                // ONNX Runtime expects inputs as 'OnnxValue' objects. 
                // We use 'DenseTensor<T>' to wrap our data.
                // Shape: [BatchSize, SequenceLength]
                // For Phi-2, the input is typically 'input_ids' (long type).
                long[] inputIdsArray = inputIds.ToArray();
                var inputTensor = new DenseTensor<long>(inputIdsArray, new[] { 1, inputIdsArray.Length });

                // Create the input container (ReadOnlySpan<byte> is used internally by the wrapper)
                var inputName = session.InputMetadata.Keys.First();
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
                };

                // --- Run Inference ---
                // We run the session. This is the heavy lifting.
                // Note: In a UI app (WPF/WinForms), this MUST be awaited on a background thread
                // to prevent freezing the interface.
                using var results = session.Run(inputs);

                // --- Post-Processing (Decoding) ---
                // Extract the output tensor. Phi-2 outputs 'logits' (floats) or 'token_ids' depending on the export.
                // We will assume the model outputs 'logits' (shape [1, seq_len, vocab_size]).
                // For simplicity here, we are extracting the last token's ID to demonstrate the flow.
                
                // In a real scenario, you would perform 'Greedy Decoding' or 'Beam Search' here:
                // 1. Get logits for the last token.
                // 2. Apply Softmax to get probabilities.
                // 3. Pick the highest probability token.
                // 4. Append it to inputIds and repeat (autoregressive generation).
                
                // To keep this example runnable and concise, we will simulate the decoding loop
                // using our mock tokenizer to demonstrate the iteration logic.
                
                var outputBuilder = new StringBuilder(prompt);
                int maxNewTokens = 20; // Limit generation to prevent infinite loops

                // We start with the current input IDs
                var currentIds = new List<long>(inputIdsArray);

                for (int i = 0; i < maxNewTokens; i++)
                {
                    // Prepare input for the next step (using the accumulated history)
                    var nextInputTensor = new DenseTensor<long>(currentIds.ToArray(), new[] { 1, currentIds.Count });
                    var nextInputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor(inputName, nextInputTensor)
                    };

                    // Run inference for the next token
                    using var nextResults = session.Run(nextInputs);
                    
                    // Get the logits (output scores for every word in the vocabulary)
                    // Shape is usually [1, sequence_length, vocab_size]
                    var logitsTensor = nextResults.First().AsTensor<float>();
                    
                    // We only care about the logits of the *last* token in the sequence
                    // The shape is [1, currentLength, vocabSize], so we slice the last index of dimension 1.
                    int vocabSize = logitsTensor.Dimensions[2];
                    int lastTokenIndex = currentIds.Count - 1;
                    
                    // Extract the slice for the last token
                    var lastTokenLogits = new float[vocabSize];
                    for (int v = 0; v < vocabSize; v++)
                    {
                        // Accessing tensor data manually. 
                        // Indices: [batch=0, sequence_position=lastTokenIndex, vocab_index=v]
                        lastTokenLogits[v] = logitsTensor[0, lastTokenIndex, v];
                    }

                    // Greedy Decoding: Find the index of the maximum logit
                    int predictedTokenId = 0;
                    float maxLogit = float.MinValue;
                    for (int v = 0; v < vocabSize; v++)
                    {
                        if (lastTokenLogits[v] > maxLogit)
                        {
                            maxLogit = lastTokenLogits[v];
                            predictedTokenId = v;
                        }
                    }

                    // Check for end-of-sequence (simulated)
                    if (predictedTokenId == tokenizer.EndOfTextTokenId)
                    {
                        break;
                    }

                    // Add the predicted token to our history and the output string
                    currentIds.Add(predictedTokenId);
                    char decodedChar = tokenizer.Decode(predictedTokenId);
                    outputBuilder.Append(decodedChar);
                }

                return outputBuilder.ToString();
            });
        }
    }

    /// <summary>
    /// A simplified mock tokenizer for demonstration purposes.
    /// Real ONNX LLMs use complex BPE/WordPiece tokenizers (e.g., HuggingFace Tokenizers).
    /// </summary>
    public class SimpleMockTokenizer
    {
        private readonly Dictionary<char, long> _charToId = new();
        private readonly Dictionary<long, char> _idToChar = new();
        private int _currentIndex = 1;
        public long EndOfTextTokenId => 0;

        public SimpleMockTokenizer()
        {
            // Initialize with basic ASCII
            for (char c = ' '; c <= '~'; c++)
            {
                _charToId[c] = _currentIndex;
                _idToChar[_currentIndex] = c;
                _currentIndex++;
            }
            // Add newline
            _charToId['\n'] = _currentIndex;
            _idToChar[_currentIndex] = '\n';
        }

        public List<long> Encode(string text)
        {
            var ids = new List<long> { EndOfTextTokenId }; // Start with BOS token
            foreach (char c in text)
            {
                if (_charToId.TryGetValue(c, out long id))
                    ids.Add(id);
                else
                    ids.Add(_charToId['?']); // Unknown char
            }
            return ids;
        }

        public char Decode(long id)
        {
            if (_idToChar.TryGetValue(id, out char c))
                return c;
            return '?';
        }
    }
}
