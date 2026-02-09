
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RawRecord { public int Id; public string Data; }
public class Embedding { public int Id; public float[] Vector; }

public class VectorPipelineOptimizer
{
    // Simulated I/O (High latency, low CPU)
    private static RawRecord FetchRecord(int id)
    {
        Task.Delay(10).Wait(); // Simulate network delay
        return new RawRecord { Id = id, Data = "some_data" };
    }

    // Simulated CPU-intensive Vector Generation
    private static Embedding GenerateVector(RawRecord record)
    {
        // Simulate heavy matrix math
        return new Embedding 
        { 
            Id = record.Id, 
            Vector = new float[] { record.Data.Length * 0.5f } 
        };
    }

    // Simulated I/O (High latency)
    private static void SaveEmbedding(Embedding emb)
    {
        Task.Delay(10).Wait();
    }

    public static void OptimizePipeline(List<int> ids)
    {
        // --- OPTIMIZED SOLUTION ---

        // Step 1: Fetch data sequentially (I/O Bound)
        // We avoid AsParallel() here to prevent thread pool exhaustion 
        // caused by blocking I/O waits.
        var records = ids.Select(id => FetchRecord(id)).ToList();

        // Step 2: Process CPU-bound work (Parallel)
        // We apply AsParallel() here where it is most effective: 
        // on the in-memory data processing.
        var embeddings = records
            .AsParallel()
            .Select(record => GenerateVector(record))
            .ToList(); 

        // Step 3: Save results (I/O Bound)
        // We use a standard loop for side effects, keeping the LINQ pipeline pure.
        foreach (var emb in embeddings)
        {
            SaveEmbedding(emb);
        }
    }
}
