
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
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
*/

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class InferenceEngine
{
    // Simulated read-only model weights (safe for concurrent reads)
    private readonly float[] _modelWeights;
    
    // Simulated KV Cache (requires synchronization)
    // Structure: A jagged array where each sub-array is a sequence's cache.
    private readonly float[][] _kvCache;
    private readonly ReaderWriterLockSlim[] _cacheLocks;

    public InferenceEngine(int modelSize, int maxSequences)
    {
        _modelWeights = new float[modelSize];
        _kvCache = new float[maxSequences][];
        _cacheLocks = new ReaderWriterLockSlim[maxSequences];
        
        // Initialize locks and cache placeholders
        for (int i = 0; i < maxSequences; i++)
        {
            _cacheLocks[i] = new ReaderWriterLockSlim();
            _kvCache[i] = new float[1024]; // Example fixed size per sequence
        }
    }

    /// <summary>
    /// Refactored Generate method accepting a list of prompts.
    /// </summary>
    public List<string> Generate(List<string> prompts, int batchSize)
    {
        var results = new List<string>();
        
        // 1. Batch the prompts
        var batches = prompts
            .Select((value, index) => new { value, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.value).ToList())
            .ToList();

        // 2. Process batches in parallel
        // We use Parallel.ForEach to process batches concurrently.
        // Note: In a real scenario, we might limit concurrency based on CPU cores.
        Parallel.ForEach(batches, batch =>
        {
            foreach (var prompt in batch)
            {
                // 3. Identify the specific slot for this prompt (simulated logic)
                int sequenceId = Math.Abs(prompt.GetHashCode()) % _kvCache.Length;

                // 4. Thread-safe access to KV Cache
                _cacheLocks[sequenceId].EnterWriteLock();
                try
                {
                    // Simulate heavy computation (matrix multiplication)
                    // Accessing shared read-only weights is safe here.
                    ComputeLayer(_kvCache[sequenceId], _modelWeights);
                    
                    // Update result
                    lock (results)
                    {
                        results.Add($"Processed: {prompt} [SeqID: {sequenceId}]");
                    }
                }
                finally
                {
                    _cacheLocks[sequenceId].ExitWriteLock();
                }
            }
        });

        return results;
    }

    private void ComputeLayer(float[] kvSlot, float[] weights)
    {
        // Simulate computation time
        Thread.SpinWait(1000); 
        // In reality: kvSlot[i] += dot_product(...)
    }
}
