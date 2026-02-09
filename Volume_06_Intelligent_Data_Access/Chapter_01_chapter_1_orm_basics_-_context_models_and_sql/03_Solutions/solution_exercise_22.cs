
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

# Source File: solution_exercise_22.cs
# Description: Solution for Exercise 22
# ==========================================

using System;
using System.Linq;

// 1. Differential Privacy Implementation
public class DifferentialPrivacy
{
    private readonly double _epsilon; // Privacy budget (lower = more privacy, less utility)

    public DifferentialPrivacy(double epsilon) => _epsilon = epsilon;

    public float[] AddNoise(float[] vector)
    {
        // Laplace Mechanism: Add noise proportional to sensitivity / epsilon
        // Sensitivity for embeddings is usually 1.0 (max change per dimension)
        var sensitivity = 1.0;
        var scale = sensitivity / _epsilon;

        var noisyVector = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
        {
            // Sample from Laplace distribution
            var noise = SampleLaplace(scale);
            noisyVector[i] = (float)(vector[i] + noise);
        }

        return noisyVector;
    }

    private double SampleLaplace(double scale)
    {
        var u = new Random().NextDouble() - 0.5;
        return -scale * Math.Sign(u) * Math.Log(1 - 2 * Math.Abs(u));
    }
}

// 2. Privacy-Aware Service
public class PrivacyPreservingSearch
{
    private readonly DifferentialPrivacy _dp;

    public async Task<List<Document>> SearchWithPrivacy(float[] queryVector, string userId)
    {
        // 1. Perturb the query vector
        // This hides the exact intent of the user
        var noisyQuery = _dp.AddNoise(queryVector);

        // 2. Execute search on noisy vector
        // Note: The database still contains raw vectors (unless we store noisy ones)
        // For true privacy, we usually add noise to the *results* or use Federated Learning.
        
        // Simulated search
        return await Task.FromResult(new List<Document>());
    }
}

// 3. Federated Learning Concept (Simulated)
// In a real scenario, the model is trained locally on user device, 
// and only gradients (not vectors) are sent to the server.
public class FederatedLearningSim
{
    public void TrainLocalModel(string userId, float[] localData)
    {
        // 1. Generate vector on device
        // 2. Add noise (Differential Privacy)
        // 3. Send only the noisy vector to server
        // 4. Server aggregates without knowing specific user data
    }
}
