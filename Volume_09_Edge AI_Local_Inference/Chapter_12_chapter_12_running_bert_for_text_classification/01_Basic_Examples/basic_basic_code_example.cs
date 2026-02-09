
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

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace BertSentimentAnalysis
{
    // This example demonstrates a complete, self-contained BERT-based sentiment analysis pipeline.
    // We will classify a short text snippet as either "Positive" or "Negative" using a DistilBERT model.
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Define the input text we want to analyze.
            string inputText = "The movie was absolutely fantastic! The acting was superb.";
            
            // 2. Load the ONNX model. 
            // NOTE: For this example to run, you must download a DistilBERT model for sequence classification
            // (e.g., from HuggingFace converted to ONNX) and place it in the execution directory.
            // We will assume the filename is "distilbert-base-uncased-finetuned-sst-2-english.onnx".
            string modelPath = "distilbert-base-uncased-finetuned-sst-2-english.onnx";
            
            // 3. Initialize the tokenizer. 
            // In a production environment, we would use a dedicated tokenizer library. 
            // Here, we implement a basic WordPiece tokenizer logic manually to keep the example self-contained.
            var tokenizer = new BasicBertTokenizer();

            // 4. Tokenize the input text.
            // This converts raw text into numerical IDs and generates necessary attention masks.
            var encodedInput = tokenizer.Encode(inputText);

            // 5. Create the ONNX Runtime Inference Session.
            // We wrap this in a 'using' statement to ensure proper disposal of unmanaged resources.
            using var session = new InferenceSession(modelPath);

            // 6. Prepare the inputs for the model.
            // BERT models typically require three inputs: 
            // - Input IDs: The tokenized text.
            // - Attention Mask: Tells the model which tokens to pay attention to (1 for real tokens, 0 for padding).
            // - Token Type IDs: Used for sentence pairs (not needed for single-sentence classification, set to 0).
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", encodedInput.InputIds),
                NamedOnnxValue.CreateFromTensor("attention_mask", encodedInput.AttentionMask),
                NamedOnnxValue.CreateFromTensor("token_type_ids", encodedInput.TokenTypeIds)
            };

            // 7. Run inference.
            // The session.Run method executes the forward pass of the neural network.
            using var results = session.Run(inputs);

            // 8. Extract and process the output.
            // The output is usually a tensor of logits (raw scores) for each class.
            var outputTensor = results.First().AsTensor<float>();
            
            // 9. Convert logits to probabilities using Softmax.
            var probabilities = Softmax(outputTensor.ToArray());

            // 10. Determine the predicted label.
            // Assuming index 0 is Negative and index 1 is Positive (standard for SST-2).
            string predictedLabel = probabilities[1] > 0.5 ? "Positive" : "Negative";

            // 11. Display the results.
            Console.WriteLine($"Input Text: \"{inputText}\"");
            Console.WriteLine($"Predicted Sentiment: {predictedLabel}");
            Console.WriteLine($"Confidence Scores: Negative={probabilities[0]:P2}, Positive={probabilities[1]:P2}");
        }

        // Helper method to calculate Softmax probabilities.
        // Softmax converts raw logits into a probability distribution summing to 1.
        static float[] Softmax(float[] logits)
        {
            var max = logits.Max();
            var exp = logits.Select(x => Math.Exp(x - max)).ToArray();
            var sum = exp.Sum();
            return exp.Select(x => (float)(x / sum)).ToArray();
        }
    }

    /// <summary>
    /// A simplified implementation of a BERT tokenizer for demonstration purposes.
    /// Real-world applications should use libraries like Microsoft.ML.Tokenizers or HuggingFace tokenizers.
    /// </summary>
    public class BasicBertTokenizer
    {
        // Basic vocabulary mapping for demonstration (truncated for brevity).
        // In a real scenario, this would be loaded from a 'vocab.txt' file.
        private readonly Dictionary<string, int> _vocab = new()
        {
            { "[CLS]", 101 }, { "[SEP]", 102 }, { "[PAD]", 0 },
            { "the", 1996 }, { "movie", 3185 }, { "was", 2001 },
            { "absolutely", 4593 }, { "fantastic", 11025 }, { "!", 999 },
            { "acting", 3724 }, { "superb", 11344 }, { "the", 1996 }
        };

        public EncodedInput Encode(string text, int maxLen = 128)
        {
            // 1. Normalize and split text into words.
            var words = text.ToLower().Split(new[] { ' ', '!', '.', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            // 2. Convert words to token IDs.
            var tokenIds = new List<int>();
            tokenIds.Add(_vocab["[CLS]"]); // Start of sequence token

            foreach (var word in words)
            {
                // Simple lookup (in reality, WordPiece tokenization splits unknown words)
                if (_vocab.TryGetValue(word, out int id))
                {
                    tokenIds.Add(id);
                }
                else
                {
                    // Handle unknown words by mapping to a generic ID or [UNK]
                    // For this example, we'll skip or map to a placeholder
                    tokenIds.Add(100); // Assuming 100 is [UNK]
                }
            }

            tokenIds.Add(_vocab["[SEP]"]); // End of sequence token

            // 3. Padding and Attention Mask creation
            // Create tensors with the specified max length.
            var inputIds = new int[maxLen];
            var attentionMask = new int[maxLen];
            var tokenTypeIds = new int[maxLen];

            // Copy values
            for (int i = 0; i < Math.Min(tokenIds.Count, maxLen); i++)
            {
                inputIds[i] = tokenIds[i];
                attentionMask[i] = 1; // 1 indicates a real token
                tokenTypeIds[i] = 0;  // 0 indicates the first sentence
            }

            // Fill the rest with Padding IDs (0)
            for (int i = tokenIds.Count; i < maxLen; i++)
            {
                inputIds[i] = _vocab["[PAD]"];
                attentionMask[i] = 0; // 0 indicates padding
                tokenTypeIds[i] = 0;
            }

            // 4. Convert to DenseTensors for ONNX Runtime
            // We need to reshape to [1, sequence_length] for BERT inputs.
            var inputIdsTensor = new DenseTensor<int>(inputIds, new[] { 1, maxLen });
            var attentionMaskTensor = new DenseTensor<int>(attentionMask, new[] { 1, maxLen });
            var tokenTypeIdsTensor = new DenseTensor<int>(tokenTypeIds, new[] { 1, maxLen });

            return new EncodedInput
            {
                InputIds = inputIdsTensor,
                AttentionMask = attentionMaskTensor,
                TokenTypeIds = tokenTypeIdsTensor
            };
        }
    }

    // Container for the encoded inputs
    public class EncodedInput
    {
        public Tensor<int> InputIds { get; set; }
        public Tensor<int> AttentionMask { get; set; }
        public Tensor<int> TokenTypeIds { get; set; }
    }
}
