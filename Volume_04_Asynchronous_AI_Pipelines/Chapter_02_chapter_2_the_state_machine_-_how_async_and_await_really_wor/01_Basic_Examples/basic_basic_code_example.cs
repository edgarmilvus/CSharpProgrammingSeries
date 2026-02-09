
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

using System;
using System.Threading.Tasks;

class AsyncStateMachineDemo
{
    static async Task Main(string[] args)
    {
        // Real-world context: An AI service needs to fetch user profile
        // and product catalog simultaneously to generate a personalized recommendation.
        Console.WriteLine("Starting personalized AI recommendation pipeline...");
        
        // This is the "Hello World" of async/await state transitions.
        // We simulate a network call to an AI model endpoint.
        string recommendation = await GetPersonalizedRecommendationAsync();
        
        Console.WriteLine($"Recommendation: {recommendation}");
        Console.WriteLine("Pipeline complete.");
    }

    static async Task<string> GetPersonalizedRecommendationAsync()
    {
        // 1. The method starts executing synchronously.
        Console.WriteLine("  [1] Fetching user profile data...");
        
        // 2. The 'await' keyword triggers a state transition.
        //    This simulates a network request (e.g., calling an LLM API).
        //    The compiler generates a state machine that pauses here.
        string userProfile = await FetchDataAsync("User Profile");
        
        // 3. Execution resumes here only after FetchDataAsync completes.
        //    The state machine transitions from 'Suspended' back to 'Running'.
        Console.WriteLine($"  [2] Received profile: {userProfile}");
        
        Console.WriteLine("  [3] Fetching product catalog...");
        
        // 4. Another await, another suspension point.
        string productCatalog = await FetchDataAsync("Product Catalog");
        
        Console.WriteLine($"  [4] Received catalog: {productCatalog}");
        
        // 5. Synchronous processing after async operations.
        return $"Based on '{userProfile}' and '{productCatalog}', buy this AI gadget!";
    }

    static async Task<string> FetchDataAsync(string source)
    {
        // Simulates an I/O-bound operation (e.g., HTTP request).
        // 'Task.Delay' yields control to the event loop without blocking the thread.
        await Task.Delay(100); 
        
        // Simulates data retrieval.
        return source switch
        {
            "User Profile" => "Tech Enthusiast",
            "Product Catalog" => "Latest Neural Processor",
            _ => "Unknown"
        };
    }
}
