
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

# Source File: solution_exercise_42.cs
# Description: Solution for Exercise 42
# ==========================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Toxicity Embeddings
public class ModerationService
{
    private readonly IVectorRepository<Content> _repo;

    // Pre-defined vectors for "Hate Speech", "Spam", "Harassment"
    private readonly float[] _hateSpeechVector = new float[128]; // Load from model

    public async Task<ModerationResult> ModerateAsync(string content)
    {
        var vector = await GenerateEmbedding(content);

        // 1. Similarity to known bad vectors
        var toxicityScore = CalculateSimilarity(vector, _hateSpeechVector);

        if (toxicityScore > 0.85)
        {
            return new ModerationResult { IsSafe = false, Reason = "Hate Speech Detected" };
        }

        // 2. Semantic Duplicate Detection
        // Check if this post is semantically identical to recently removed posts
        var duplicates = await _repo.SearchAsync(vector, 5);
        if (duplicates.Any(d => d.IsRemoved))
        {
            return new ModerationResult { IsSafe = false, Reason = "Duplicate of removed content" };
        }

        return new ModerationResult { IsSafe = true };
    }

    private double CalculateSimilarity(float[] a, float[] b) => 0.9; // Mock
}

public class ModerationResult
{
    public bool IsSafe { get; set; }
    public string Reason { get; set; }
}
