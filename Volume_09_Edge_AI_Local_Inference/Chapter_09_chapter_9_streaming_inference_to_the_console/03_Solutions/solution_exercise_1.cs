
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StreamingEngine
{
    // Mock tokenizer for demonstration. In production, use a proper tokenizer library.
    private Dictionary<string, int> _vocab = new Dictionary<string, int> { { "The", 1 }, { " cat", 2 }, { " sat", 3 }, { " on", 4 }, { " the", 5 }, { " mat", 6 }, { "", 0 } };
    
    // Mock detokenizer
    private Dictionary<int, string> _reverseVocab => _vocab.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public async IAsyncEnumerable<string> GenerateTokensAsync(string prompt, InferenceSession session)
    {
        // 1. Tokenize the prompt
        // Note: In a real scenario, we would feed the prompt to the model to warm up the KV cache,
        // but for this basic greedy loop, we simulate starting after the prompt.
        // We will start generating the *next* token.
        
        // Simulate input token IDs (e.g., based on prompt)
        var inputTokens = new List<int> { 1 }; // Assume "The" was the prompt
        
        // Max generation limit
        int maxTokens = 10;
        int currentTokenCount = 0;

        while (currentTokenCount < maxTokens)
        {
            // 2. Prepare Input Tensor
            // Shape: [1, sequence_length]
            var inputTensor = new DenseTensor<long>(new long[] { 1, inputTokens.Count });
            for (int i = 0; i < inputTokens.Count; i++)
            {
                inputTensor[0, i] = inputTokens[i];
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
            };

            // 3. Run ONNX Session
            // We use RunAsync to avoid blocking the thread
            using var outputs = await Task.Run(() => session.Run(inputs));
            
            // 4. Extract Logits
            var logitsTensor = outputs.First().AsTensor<float>();
            
            // 5. Greedy Decoding (ArgMax)
            // We look at the logits for the *last* position in the sequence
            int vocabSize = logitsTensor.Dimensions[2];
            int lastPosIndex = logitsTensor.Dimensions[1] - 1;
            
            float maxLogit = float.MinValue;
            int predictedId = 0;

            for (int i = 0; i < vocabSize; i++)
            {
                float logit = logitsTensor[0, lastPosIndex, i];
                if (logit > maxLogit)
                {
                    maxLogit = logit;
                    predictedId = i;
                }
            }

            // 6. Check for End of Sequence
            if (predictedId == 0) // Assuming 0 is EOS
            {
                yield break;
            }

            // 7. Yield Result
            string tokenString = _reverseVocab.TryGetValue(predictedId, out var str) ? str : "[UNK]";
            yield return tokenString;

            // 8. Update State for next iteration
            inputTokens.Add(predictedId);
            currentTokenCount++;
        }
    }
}
