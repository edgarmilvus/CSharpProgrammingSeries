
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

public static void RunParallelPreprocessing()
{
    // Simulate a larger dataset
    var rawData = Enumerable.Range(0, 1000).Select(i => new TextEmbedding 
    { 
        Text = $"Doc {i}", 
        Vector = new float[] { i * 0.1f, i * 0.2f, i * 0.3f } 
    }).ToList();

    var sw = System.Diagnostics.Stopwatch.StartNew();

    var normalizedQuery = rawData
        .AsParallel() // Enables parallelization
        .WithDegreeOfParallelism(4) // Explicitly limit to 4 threads
        .AsOrdered() // Preserves the original order of the source collection
        .Where(e => e.Vector != null && e.Vector.Length > 0)
        .Select(e =>
        {
            // Simulate heavy calculation
            // Thread.Sleep(1); 
            
            float sumSquares = e.Vector.Sum(c => c * c);
            float length = (float)Math.Sqrt(sumSquares);
            var norm = e.Vector.Select(v => v / length).ToArray();
            
            return new { Text = e.Text, NormalizedVector = norm };
        });

    // Immediate Execution triggers the parallel processing
    var results = normalizedQuery.ToList();

    sw.Stop();
    Console.WriteLine($"Processed {results.Count} items in {sw.ElapsedMilliseconds}ms.");
    
    // Verify order is preserved due to AsOrdered()
    Console.WriteLine($"First item: {results.First().Text}");
}
