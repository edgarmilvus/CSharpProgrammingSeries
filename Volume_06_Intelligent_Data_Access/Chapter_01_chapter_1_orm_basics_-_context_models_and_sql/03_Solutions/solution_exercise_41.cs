
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

# Source File: solution_exercise_41.cs
# Description: Solution for Exercise 41
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

// 1. Multi-Vector Strategy
public class ProductVectorService
{
    // We need multiple vectors per product:
    // 1. Description Vector (Semantic)
    // 2. Image Vector (Visual)
    // 3. User Behavior Vector (Collaborative Filtering)

    public async Task<List<Product>> RecommendAsync(string userId)
    {
        // 1. Get User Profile Vector (Aggregated behavior)
        var userVector = await GetUserBehaviorVectorAsync(userId);

        // 2. Search using User Vector
        // This finds products that "look like" what the user usually buys
        var candidates = await _repo.SearchAsync(userVector, 20);

        // 3. Personalization Layer (Post-processing)
        // Boost items that match user preferences (e.g., brand, price range)
        return candidates
            .Where(p => p.Price <= GetMaxBudget(userId))
            .OrderByDescending(p => p.Similarity)
            .ToList();
    }

    private async Task<float[]> GetUserBehaviorVectorAsync(string userId)
    {
        // Aggregate past purchases and clicks into a single vector
        return new float[128];
    }

    private decimal GetMaxBudget(string userId) => 1000; // Mock
}
