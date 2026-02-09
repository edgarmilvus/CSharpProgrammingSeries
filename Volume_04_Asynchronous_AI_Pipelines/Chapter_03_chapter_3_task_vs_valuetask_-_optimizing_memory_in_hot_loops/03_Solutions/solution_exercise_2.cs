
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Threading.Tasks;

public class LlmStreamer
{
    public async ValueTask<int> GetNextTokenIdAsync()
    {
        // Simulate async work
        await Task.Yield();
        return new Random().Next(1, 100);
    }

    public async Task ProcessStreamAsync()
    {
        // FIXED: We iterate 5 times, creating a fresh ValueTask each time.
        for (int i = 0; i < 5; i++)
        {
            // We await immediately. The result of GetNextTokenIdAsync() is a temporary 
            // ValueTask struct. Awaiting it immediately is safe.
            int id = await GetNextTokenIdAsync();
            Console.WriteLine($"Token: {id}");
        }
    }

    // Alternative Fix: If we needed to store the task for some reason (e.g., race conditions),
    // we would convert it to a Task immediately using .AsTask().
    public async Task ProcessStreamAsync_AlternativeFix()
    {
        // Create the first task
        Task<int> task = GetNextTokenIdAsync().AsTask();

        for (int i = 0; i < 5; i++)
        {
            // Await the stored Task
            int id = await task;
            Console.WriteLine($"Token: {id}");

            // Prepare the next task for the next iteration
            task = GetNextTokenIdAsync().AsTask();
        }
    }
}
