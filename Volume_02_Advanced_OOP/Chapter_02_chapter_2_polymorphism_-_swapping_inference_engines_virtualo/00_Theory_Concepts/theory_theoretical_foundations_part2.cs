
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

// Concrete implementation 1: A cloud-based model
public class OpenAIEngine : IInferenceEngine
{
    private readonly string _apiKey;

    public OpenAIEngine(string apiKey)
    {
        _apiKey = apiKey;
    }

    public string ModelName => "GPT-4";

    // The override implementation for OpenAI
    public string GenerateResponse(string prompt)
    {
        // Simulate API call logic
        Console.WriteLine($"Connecting to OpenAI with key: {_apiKey.Substring(0, 5)}...");
        return $"[OpenAI GPT-4]: Processed '{prompt}'";
    }
}

// Concrete implementation 2: A local model
public class LocalLlamaEngine : IInferenceEngine
{
    private readonly int _gpuLayers;

    public LocalLlamaEngine(int gpuLayers)
    {
        _gpuLayers = gpuLayers;
    }

    public string ModelName => "Llama-3-8B";

    // The override implementation for Local Llama
    public string GenerateResponse(string prompt)
    {
        // Simulate local inference logic
        Console.WriteLine($"Loading {_gpuLayers} layers onto GPU...");
        return $"[Local Llama]: Processed '{prompt}'";
    }
}
