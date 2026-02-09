
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

# Source File: theory_theoretical_foundations_part5.cs
# Description: Theoretical Foundations
# ==========================================

public class DataPipeline
{
    public static IEnumerable<float[]> PrepareBatch(IEnumerable<float[]> rawData, int batchSize)
    {
        // 1. CLEANING: Remove rows with missing values (NaN)
        // .Where filters the stream
        var cleanData = rawData.Where(row => !row.Contains(float.NaN));

        // 2. NORMALIZATION: Scale inputs between 0 and 1
        // .Select projects each row into a normalized row
        // Note: In a real scenario, we'd calculate min/max first (Immediate Execution)
        var maxVal = cleanData.Max(row => row.Max()); 
        var normalizedData = cleanData.Select(row => 
            row.Select(val => val / maxVal).ToArray()
        );

        // 3. SHUFFLING: Randomize the order
        // We use a seeded random to ensure reproducibility in training
        var rnd = new Random(42);
        var shuffled = normalizedData.OrderBy(x => rnd.Next());

        // 4. BATCHING: Group into chunks
        // .SelectMany with Index allows us to group by batch index
        var batched = shuffled
            .Select((value, index) => new { value, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.value).ToList());

        return batched;
    }
}
