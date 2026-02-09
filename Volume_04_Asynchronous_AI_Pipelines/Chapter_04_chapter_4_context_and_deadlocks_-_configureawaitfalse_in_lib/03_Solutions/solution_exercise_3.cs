
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class StreamingPipeline
{
    private readonly IAsyncEnumerable<string> _sourceStream;

    public StreamingPipeline(IAsyncEnumerable<string> sourceStream)
    {
        _sourceStream = sourceStream;
    }

    public async Task ConsumeStreamAsync()
    {
        // Using await foreach is generally context-aware by default.
        // However, for pure processing, we can rely on the internal awaits to handle context.
        await foreach (var chunk in _sourceStream)
        {
            // 1. Tokenize (CPU intensive) - No context needed
            var tokens = await TokenizeAsync(chunk).ConfigureAwait(false);
            
            // 2. Buffer (Waiting for semantic coherence) - No context needed
            var buffer = await BufferAsync(tokens).ConfigureAwait(false);
            
            // 3. Render (UI update) - Context NEEDED (Do not use ConfigureAwait(false))
            // If we used ConfigureAwait(false) here, the Console.WriteLine might run on a ThreadPool thread.
            // In a real WinForms/WPF app, this would throw an exception or fail to update UI.
            await RenderTokenAsync(buffer); 
        }
    }

    private async Task<string> TokenizeAsync(string chunk)
    {
        // Simulate CPU work
        await Task.Delay(10).ConfigureAwait(false);
        return chunk.ToUpper();
    }

    private async Task<string> BufferAsync(string tokens)
    {
        // Simulate Network buffering
        await Task.Delay(5).ConfigureAwait(false);
        return tokens;
    }

    private async Task RenderTokenAsync(string text)
    {
        // Simulate UI update delay
        await Task.Delay(1); 
        Console.WriteLine($"Rendering: {text}");
    }
}

// Helper for the Interactive Challenge
public static class PipelineRunner
{
    public static async Task RunBenchmark()
    {
        // Mock 100 items
        var source = AsyncEnumerable.Range(0, 100)
            .Select(i => $"chunk_{i}");

        var pipeline = new StreamingPipeline(source);

        var sw = Stopwatch.StartNew();
        await pipeline.ConsumeStreamAsync();
        sw.Stop();

        Console.WriteLine($"Total execution time: {sw.ElapsedMilliseconds}ms");
    }
}
