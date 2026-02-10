
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
using System.Text.RegularExpressions;

namespace BertEdgeExercises
{
    // 1. Create a record to hold the tensor data
    public record BertInput(
        long[] InputIds,
        long[] AttentionMask,
        long[] TokenTypeIds,
        int SequenceLength
    );

    public class ManualTokenizer
    {
        private readonly Dictionary<string, long> _vocab;
        private readonly int _maxSeqLen;
        private readonly long _padTokenId = 0;
        private readonly long _clsTokenId = 101;
        private readonly long _sepTokenId = 102;
        private readonly long _unkTokenId = 100;

        public ManualTokenizer(Dictionary<string, long> vocab, int maxSeqLen = 128)
        {
            _vocab = vocab;
            _maxSeqLen = maxSeqLen;
        }

        public BertInput TokenizeBatch(string[] sentences)
        {
            // In a real scenario, we might pad to the longest in the batch. 
            // For this exercise, we pad to the fixed _maxSeqLen.
            int batchSize = sentences.Length;
            int totalSize = batchSize * _maxSeqLen;

            var inputIds = new long[totalSize];
            var attentionMask = new long[totalSize];
            var tokenTypeIds = new long[totalSize]; // DistilBERT usually ignores this, but we fill it.

            for (int i = 0; i < batchSize; i++)
            {
                var sentence = sentences[i];
                var tokens = Tokenize(sentence); // Split and map to IDs
                
                // Calculate offsets
                int offset = i * _maxSeqLen;
                
                // Add [CLS] token
                inputIds[offset] = _clsTokenId;
                attentionMask[offset] = 1;

                int currentTokenIdx = 1; // Start after [CLS]

                // Fill actual tokens
                foreach (var tokenId in tokens)
                {
                    if (currentTokenIdx < _maxSeqLen - 1) // Leave room for [SEP]
                    {
                        inputIds[offset + currentTokenIdx] = tokenId;
                        attentionMask[offset + currentTokenIdx] = 1;
                        currentTokenIdx++;
                    }
                }

                // Add [SEP] token
                inputIds[offset + currentTokenIdx] = _sepTokenId;
                attentionMask[offset + currentTokenIdx] = 1;
                
                // Padding is implicitly handled by array initialization (zeros)
                // TokenTypeIds remain 0 for DistilBERT
            }

            return new BertInput(inputIds, attentionMask, tokenTypeIds, _maxSeqLen);
        }

        private List<long> Tokenize(string text)
        {
            // Simulate WordPiece: Lowercase, Regex split on word boundaries
            text = text.ToLower();
            // Simple regex to split by whitespace and punctuation
            var rawTokens = Regex.Split(text, @"\b").Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            
            var tokenIds = new List<long>();
            foreach (var token in rawTokens)
            {
                // In a real WordPiece, we'd check for subwords (e.g., "##ing").
                // Here we just look up the token or return UNK.
                if (_vocab.TryGetValue(token, out long id))
                {
                    tokenIds.Add(id);
                }
                else
                {
                    tokenIds.Add(_unkTokenId);
                }
            }
            return tokenIds;
        }
    }
}
