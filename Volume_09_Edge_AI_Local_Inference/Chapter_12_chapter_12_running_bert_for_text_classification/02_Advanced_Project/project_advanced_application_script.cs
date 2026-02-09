
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
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace RealTimeSupportTicketClassifier
{
    class Program
    {
        // Configuration constants for the application
        const string ModelPath = "distilbert-base-uncased-finetuned-sst-2-english.onnx";
        const int MaxSequenceLength = 128;
        const int VocabularySize = 30522; // Standard BERT vocab size

        static void Main(string[] args)
        {
            Console.WriteLine("--- Real-Time Support Ticket Classifier ---");
            Console.WriteLine("Initializing ONNX Runtime Session...");

            // 1. Initialize the ONNX Runtime Inference Session
            // We wrap this in a try-catch to handle missing model files gracefully
            InferenceSession session;
            try
            {
                var sessionOptions = new SessionOptions();
                // Default execution provider (CPU). On Windows, this uses DirectML automatically.
                session = new InferenceSession(ModelPath, sessionOptions);
                Console.WriteLine("Model loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading model: {ex.Message}");
                Console.WriteLine("Please ensure 'distilbert-base-uncased-finetuned-sst-2-english.onnx' is in the executable directory.");
                return;
            }

            // 2. Load Tokenizer Vocabulary (Mock Implementation)
            // In a production scenario, we would load a vocab.json file.
            // For this example, we simulate a small subset of the vocabulary.
            var vocab = LoadVocabulary();

            // 3. Start the Real-Time Input Loop
            while (true)
            {
                Console.Write("\nEnter support ticket text (or 'exit' to quit): ");
                string inputText = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(inputText)) continue;
                if (inputText.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                // 4. Preprocessing: Tokenize and Convert to IDs
                // BERT requires specific input: InputIds, AttentionMask, TokenTypeIds
                var processedInput = PreprocessText(inputText, vocab);

                // 5. Prepare Input Tensors for ONNX Runtime
                // We create DenseTensors to hold the numerical data
                using var inputIdsTensor = new DenseTensor<long>(processedInput.InputIds, new[] { 1, processedInput.InputIds.Length });
                using var attentionMaskTensor = new DenseTensor<long>(processedInput.AttentionMask, new[] { 1, processedInput.AttentionMask.Length });
                using var tokenTypeIdsTensor = new DenseTensor<long>(processedInput.TokenTypeIds, new[] { 1, processedInput.TokenTypeIds.Length });

                // 6. Create Input Container (List of NamedOnnxValue)
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
                    NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor)
                };

                // 7. Run Inference
                // 'using' statement ensures resources are freed immediately after execution
                using var results = session.Run(inputs);

                // 8. Post-Processing: Extract Logits and Softmax
                // The model outputs raw logits (scores) for [Negative, Positive]
                var outputTensor = results[0].AsTensor<float>();
                var scores = Softmax(outputTensor.ToArray());

                // 9. Interpret Results
                string sentiment = scores[1] > 0.5 ? "POSITIVE (Happy)" : "NEGATIVE (Angry/Urgent)";
                float confidence = scores[1] > 0.5 ? scores[1] : scores[0];

                // 10. Output Decision
                Console.WriteLine($"Result: {sentiment}");
                Console.WriteLine($"Confidence: {confidence:P2}");
                Console.WriteLine($"Raw Scores [Neg, Pos]: [{scores[0]:F4}, {scores[1]:F4}]");
            }

            Console.WriteLine("Application terminated.");
        }

        // ---------------------------------------------------------
        // BLOCK 1: Vocabulary Loader
        // ---------------------------------------------------------
        static Dictionary<string, int> LoadVocabulary()
        {
            // In a real BERT implementation, the vocabulary contains 30,522 tokens.
            // We create a minimal mock dictionary for demonstration purposes.
            // This maps text tokens (strings) to integer IDs required by the model.
            var vocab = new Dictionary<string, int>
            {
                { "[PAD]", 0 },
                { "[CLS]", 101 },
                { "[SEP]", 102 },
                { "[UNK]", 103 },
                { "i", 1045 },
                { "love", 2022 },
                { "hate", 3408 },
                { "this", 2023 },
                { "service", 2327 },
                { "product", 3192 },
                { "bad", 2987 },
                { "good", 2204 },
                { "terrible", 6654 },
                { "amazing", 6426 },
                { "issue", 3271 },
                { "problem", 3272 },
                { "resolved", 5026 },
                { "broken", 3718 },
                { "slow", 4353 },
                { "fast", 3434 },
                { "support", 2490 },
                { "team", 2136 },
                { "wait", 3524 },
                { "time", 2051 },
                { "refund", 15541 },
                { "money", 2769 },
                { "back", 2067 },
                { "want", 2215 },
                { "need", 2342 },
                { "help", 2393 }
            };
            return vocab;
        }

        // ---------------------------------------------------------
        // BLOCK 2: Text Preprocessing (Tokenizer Logic)
        // ---------------------------------------------------------
        static ProcessedInput PreprocessText(string text, Dictionary<string, int> vocab)
        {
            // Step 2.1: Basic Text Normalization
            // Convert to lowercase and remove simple punctuation for the mock tokenizer.
            string cleanText = text.ToLower();
            char[] punctuation = { '.', ',', '!', '?', ':', ';', '"', '(', ')' };
            foreach (var p in punctuation) cleanText = cleanText.Replace(p.ToString(), "");

            // Step 2.2: Tokenization (Split by space)
            string[] tokens = cleanText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Step 2.3: Convert Tokens to IDs
            // We use a list to dynamically build the sequence.
            var ids = new List<int>();
            
            // Add [CLS] token (ID 101) - Start of sequence indicator
            ids.Add(vocab.ContainsKey("[CLS]") ? vocab["[CLS]"] : 1);

            foreach (var token in tokens)
            {
                // Look up token in vocabulary. If not found, use [UNK] (Unknown) token.
                if (vocab.ContainsKey(token))
                {
                    ids.Add(vocab[token]);
                }
                else
                {
                    ids.Add(vocab.ContainsKey("[UNK]") ? vocab["[UNK]"] : 0);
                }
            }

            // Add [SEP] token (ID 102) - Separator between sentence and padding
            ids.Add(vocab.ContainsKey("[SEP]") ? vocab["[SEP]"] : 2);

            // Step 2.4: Padding and Truncation
            // BERT models expect fixed-size input (e.g., 128 tokens).
            var inputIds = new long[MaxSequenceLength];
            var attentionMask = new long[MaxSequenceLength];
            var tokenTypeIds = new long[MaxSequenceLength]; // 0 for single sentence

            for (int i = 0; i < MaxSequenceLength; i++)
            {
                if (i < ids.Count)
                {
                    // Copy actual token IDs
                    inputIds[i] = ids[i];
                    // Attention Mask: 1 for real tokens, 0 for padding
                    attentionMask[i] = 1;
                    // Token Type IDs: 0 for single sequence
                    tokenTypeIds[i] = 0;
                }
                else
                {
                    // Padding: 0 for InputIds, 0 for AttentionMask
                    inputIds[i] = 0;
                    attentionMask[i] = 0;
                    tokenTypeIds[i] = 0;
                }
            }

            return new ProcessedInput
            {
                InputIds = inputIds,
                AttentionMask = attentionMask,
                TokenTypeIds = tokenTypeIds
            };
        }

        // ---------------------------------------------------------
        // BLOCK 3: Softmax Calculation (Post-Processing)
        // ---------------------------------------------------------
        static float[] Softmax(float[] logits)
        {
            // The model outputs 'logits' (raw, unnormalized scores).
            // We need to convert these to probabilities using the Softmax function.
            // Formula: exp(x_i) / sum(exp(x_j))
            
            float maxLogit = float.MinValue;
            
            // Find the maximum value for numerical stability (prevents overflow)
            foreach (var logit in logits)
            {
                if (logit > maxLogit) maxLogit = logit;
            }

            // Calculate exponentials and sum
            float sum = 0.0f;
            float[] expValues = new float[logits.Length];
            
            for (int i = 0; i < logits.Length; i++)
            {
                expValues[i] = (float)Math.Exp(logits[i] - maxLogit); // Subtract max for stability
                sum += expValues[i];
            }

            // Normalize to get probabilities
            float[] probabilities = new float[logits.Length];
            for (int i = 0; i < logits.Length; i++)
            {
                probabilities[i] = expValues[i] / sum;
            }

            return probabilities;
        }
    }

    // ---------------------------------------------------------
    // DATA STRUCTURE
    // Holds the processed numerical data ready for the neural network.
    // ---------------------------------------------------------
    public class ProcessedInput
    {
        public long[] InputIds { get; set; }
        public long[] AttentionMask { get; set; }
        public long[] TokenTypeIds { get; set; }
    }
}
