
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class EmbeddingGenerator : IDisposable
{
    private readonly InferenceSession _session;
    private readonly Dictionary<string, int> _vocab;
    private readonly int _maxSequenceLength = 128;
    private readonly Random _rng = new Random(42);

    public EmbeddingGenerator(string modelPath)
    {
        // 1. Initialize Vocabulary (Simulated BPE)
        _vocab = new Dictionary<string, int>();
        var commonWords = new[] { "the", "be", "to", "of", "and", "a", "in", "that", "have", "i", "it", "for", "not", "on", "with", "he", "as", "you", "do", "at", "this", "but", "his", "by", "from", "they", "we", "say", "her", "she", "or", "an", "will", "my", "one", "all", "would", "there", "their", "what", "so", "up", "out", "if", "about", "who", "get", "which", "go", "me", "when", "make", "can", "like", "time", "no", "just", "him", "know", "take", "people", "into", "year", "your", "good", "some", "could", "them", "see", "other", "than", "then", "now", "look", "only", "come", "its", "over", "think", "also", "back", "after", "use", "two", "how", "our", "work", "first", "well", "way", "even", "new", "want", "because", "any", "these", "give", "day", "most", "us" };
        foreach (var word in commonWords)
        {
            // Map common words to random integers 0-999
            _vocab[word] = _rng.Next(0, 1000);
        }

        // 2. Initialize ONNX Session
        var sessionOptions = new SessionOptions();
        sessionOptions.AppendExecutionProvider_CPU(0); // Force CPU
        _session = new InferenceSession(modelPath, sessionOptions);
    }

    public float[] GenerateEmbedding(string text)
    {
        // 1. Tokenize
        var tokens = Tokenize(text);

        // 2. Pad/Truncate
        var inputIds = new long[_maxSequenceLength];
        Array.Fill(inputIds, 0L); // Padding token ID is usually 0
        
        int copyLength = Math.Min(tokens.Count, _maxSequenceLength);
        for (int i = 0; i < copyLength; i++)
        {
            inputIds[i] = tokens[i];
        }

        // 3. Create Input Tensor
        // Shape: [1, sequence_length] for batch size 1
        var inputTensor = new DenseTensor<long>(inputIds, new[] { 1, _maxSequenceLength });

        // 4. Prepare Input
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
        };

        // 5. Run Inference
        using (var results = _session.Run(inputs))
        {
            // 6. Output Processing
            // Assuming output name is "last_hidden_state" or similar, checking first output
            var outputTensor = results.First().AsTensor<float>();
            
            // Result shape is usually [1, 384]. We extract the vector.
            var embedding = new float[384];
            
            // Efficient copy from tensor to array
            outputTensor.CopyTo(embedding);
            
            return embedding;
        }
    }

    private List<int> Tokenize(string text)
    {
        // Simple whitespace tokenizer mapping to vocab IDs
        var tokens = new List<int>();
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            if (_vocab.TryGetValue(word.ToLower(), out int id))
            {
                tokens.Add(id);
            }
            else
            {
                tokens.Add(_rng.Next(0, 1000)); // Unknown word mapping
            }
        }
        return tokens;
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
