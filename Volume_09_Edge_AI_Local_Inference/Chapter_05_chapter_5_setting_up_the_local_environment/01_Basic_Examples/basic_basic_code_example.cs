
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

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EdgeAIHelloWorld
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. SETUP: Define the model path and ensure the directory exists.
            // In a real app, this would be a configurable setting.
            string modelDirectory = Path.Combine(Environment.CurrentDirectory, "models");
            string modelPath = Path.Combine(modelDirectory, "phi-3-mini-4k-instruct-onnx", "cpu_and_mobile", "cpu-int4-rtn-block-32-acc-level-4.onnx");
            
            Directory.CreateDirectory(modelDirectory);
            
            Console.WriteLine($"[1] Checking for model at: {modelPath}");

            // 2. MODEL LOADING: Verify the model exists or download it if missing.
            // This ensures the example is self-contained and runnable immediately.
            if (!File.Exists(modelPath))
            {
                Console.WriteLine("[2] Model not found. Starting download...");
                // NOTE: In a production environment, you would cache this model locally 
                // or use a secure artifact repository.
                await DownloadModelAsync(modelPath);
            }
            else
            {
                Console.WriteLine("[2] Model found locally.");
            }

            // 3. SESSION CREATION: Initialize the ONNX Runtime Inference Session.
            // We use 'using' statements to ensure proper disposal of unmanaged resources.
            // We explicitly target the CPU execution provider for maximum compatibility.
            Console.WriteLine("[3] Initializing ONNX Runtime Session...");
            var sessionOptions = new SessionOptions();
            sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;
            
            // For GPU acceleration (DirectML on Windows or CUDA on Linux/NVIDIA), 
            // you would change this line:
            // var sessionOptions = SessionOptions.MakeSessionOptionWithCudaProvider(); 
            // OR
            // var sessionOptions = SessionOptions.MakeSessionOptionWithDirectMLProvider();
            
            using var inferenceSession = new InferenceSession(modelPath, sessionOptions);

            // 4. TOKENIZATION: Convert text input into numerical tokens.
            // LLMs do not process strings directly; they process integers (token IDs).
            // We will simulate a tokenizer for this "Hello World" example.
            // Real-world usage requires a dedicated tokenizer library (e.g., Microsoft.ML.Tokenizers).
            string prompt = "Explain the concept of Edge AI in one sentence.";
            Console.WriteLine($"\n[4] Input Prompt: \"{prompt}\"");
            
            var tokenizer = new SimpleTokenizer();
            var inputTokens = tokenizer.Encode(prompt);
            
            // 5. PREPARE TENSORS: Convert token lists into ONNX Runtime tensors.
            // ONNX Runtime expects specific input shapes (dimensions).
            // For this model, inputs are usually: input_ids (1, sequence_length) and attention_mask (1, sequence_length).
            var inputIds = new DenseTensor<long>(inputTokens.Select(t => (long)t).ToArray(), [1, inputTokens.Count]);
            var attentionMask = new DenseTensor<long>(Enumerable.Repeat(1L, inputTokens.Count).ToArray(), [1, inputTokens.Count]);
            
            // 6. BUILD INPUT CONTAINER: Map input names to tensors.
            // The input names (e.g., "input_ids") must match the ONNX model's graph definition exactly.
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
            };

            // 7. INFERENCE: Execute the forward pass (local inference).
            Console.WriteLine("[5] Running inference...");
            using var results = inferenceSession.Run(inputs);

            // 8. POST-PROCESSING: Extract the output logits.
            // The model outputs 'logits' (raw scores) for the next token prediction.
            var outputTensor = results.First().AsTensor<long>();
            
            // 9. DECODING: Convert token IDs back to text.
            // We take the most likely token (greedy decoding) for simplicity.
            Console.WriteLine("\n[6] Generated Output Tokens:");
            var outputTokens = outputTensor.ToArray();
            
            string decodedResponse = tokenizer.Decode(outputTokens);
            
            Console.WriteLine($"\n>>> RESULT: {decodedResponse}");
        }

        static async Task DownloadModelAsync(string destinationPath)
        {
            // Hugging Face URL for a standard ONNX Phi-3 Mini model (CPU)
            // Note: URLs change. In production, use a versioned link or hash verification.
            string url = "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-onnx/resolve/main/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4.onnx";
            
            using var httpClient = new HttpClient();
            // Important: Set a reasonable timeout for large downloads
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            // Create the directory structure if it doesn't exist
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);
            
            Console.WriteLine($"[Download Complete] Model saved to {destinationPath}");
        }
    }

    /// <summary>
    /// A minimalistic tokenizer for demonstration purposes.
    /// In a real application, use Microsoft.ML.Tokenizers or HuggingFace tokenizers.
    /// This class simulates the mapping between text and integers.
    /// </summary>
    public class SimpleTokenizer
    {
        private readonly Dictionary<string, int> _vocab;
        private readonly Dictionary<int, string> _invVocab;

        public SimpleTokenizer()
        {
            // A tiny vocabulary for demonstration. 
            // Real models have 32,000+ tokens.
            _vocab = new Dictionary<string, int>
            {
                { "<s>", 1 }, { "</s>", 2 }, { "<unk>", 3 },
                { "Explain", 100 }, { "the", 101 }, { "concept", 102 },
                { "of", 103 }, { "Edge", 104 }, { "AI", 105 },
                { "in", 106 }, { "one", 107 }, { "sentence", 108 },
                { ".", 109 }, { "is", 110 }, { "a", 111 },
                { "technology", 112 }, { "that", 113 }, { "processes", 114 },
                { "data", 115 }, { "locally", 116 }, { "on", 117 },
                { "devices", 118 }, { "rather", 119 }, { "than", 120 },
                { "cloud", 121 }, { "servers", 122 }
            };

            _invVocab = _vocab.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }

        public int[] Encode(string text)
        {
            // Simple whitespace tokenization for demo
            var tokens = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var ids = new List<int> { 1 }; // Start with <s>
            
            foreach (var token in tokens)
            {
                // Clean punctuation for matching
                string cleanToken = token.Trim('.', ',', '!', '?');
                if (_vocab.TryGetValue(cleanToken, out int id))
                {
                    ids.Add(id);
                }
                else
                {
                    ids.Add(3); // <unk>
                }
            }
            ids.Add(2); // End with </s>
            return ids.ToArray();
        }

        public string Decode(int[] ids)
        {
            var words = new List<string>();
            foreach (var id in ids)
            {
                if (id == 1 || id == 2) continue; // Skip start/end tokens
                if (_invVocab.TryGetValue(id, out string? word))
                {
                    words.Add(word);
                }
                else
                {
                    words.Add("[UNK]");
                }
            }
            return string.Join(" ", words);
        }
    }
}
