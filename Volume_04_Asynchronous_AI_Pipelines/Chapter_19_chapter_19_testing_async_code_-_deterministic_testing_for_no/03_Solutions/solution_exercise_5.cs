
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace AsyncTestingSolutions
{
    public interface ILlmClient
    {
        Task<string> ClassifyAsync(string text);
    }

    public interface ITextNormalizer
    {
        string Normalize(string input);
    }

    public class TextClassifier
    {
        private readonly ILlmClient _llmClient;
        private readonly ITextNormalizer _normalizer;

        public TextClassifier(ILlmClient llmClient, ITextNormalizer normalizer)
        {
            _llmClient = llmClient;
            _normalizer = normalizer;
        }

        public async Task<string> ClassifyAsync(string text)
        {
            var rawResponse = await _llmClient.ClassifyAsync(text);
            return _normalizer.Normalize(rawResponse);
        }
    }

    // 3. Deterministic LLM Mock using Hashing
    public class DeterministicLlmMock : ILlmClient
    {
        public Task<string> ClassifyAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Task.FromResult("Neutral"); // Default for empty
            }

            // Generate deterministic hash
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            var hashByte = bytes[0]; // Take first byte for simplicity

            // Map hash to category
            var category = hashByte % 3 switch
            {
                0 => "Positive",
                1 => "Negative",
                2 => "Neutral",
                _ => "Neutral"
            };

            // Simulate non-standard casing from LLM
            var response = category.ToLower(); 
            return Task.FromResult(response);
        }
    }

    public class StandardNormalizer : ITextNormalizer
    {
        public string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Neutral";
            // Capitalize first letter
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }

    public class ClassifierTests
    {
        private readonly TextClassifier _classifier;
        private readonly DeterministicLlmMock _mockLlm;
        private readonly StandardNormalizer _normalizer;

        public ClassifierTests()
        {
            _mockLlm = new DeterministicLlmMock();
            _normalizer = new StandardNormalizer();
            _classifier = new TextClassifier(_mockLlm, _normalizer);
        }

        // 4a. Exact matches / Deterministic mapping
        [Theory]
        [InlineData("Great service", "Positive")] // Hash of "Great service" maps to 0
        [InlineData("BAD EXPERIENCE", "Negative")] // Hash maps to 1
        [InlineData("It was okay", "Neutral")]     // Hash maps to 2
        public async Task ClassifyAsync_MappedInputs_ReturnsExpected(string input, string expected)
        {
            // Act
            var result = await _classifier.ClassifyAsync(input);

            // Assert
            Assert.Equal(expected, result);
        }

        // 4d. Edge cases
        [Fact]
        public async Task ClassifyAsync_EmptyInput_ReturnsNeutral()
        {
            var result = await _classifier.ClassifyAsync("");
            Assert.Equal("Neutral", result);
        }

        // 6. Property-Based Testing
        [Property]
        public Property Classifier_ShouldNeverCrash_And_AlwaysNormalize()
        {
            return Prop.ForAll(Arb.Default.String().Filter(s => s != null), async (string input) =>
            {
                try
                {
                    var result = await _classifier.ClassifyAsync(input);
                    
                    // Property: Result should be one of the three standard categories
                    var isValid = new[] { "Positive", "Negative", "Neutral" }.Contains(result);
                    
                    // Property: Result should be properly capitalized
                    var isCapitalized = char.IsUpper(result[0]);

                    return isValid && isCapitalized;
                }
                catch
                {
                    return false; // Should not crash
                }
            });
        }
    }
}
