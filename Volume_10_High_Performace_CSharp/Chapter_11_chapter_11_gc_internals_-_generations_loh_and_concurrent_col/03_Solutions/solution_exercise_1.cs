
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GCInternalsExercise1
{
    // Base class to provide a unique ID for tracking
    public abstract class TrackedObject
    {
        public int Id { get; }
        protected TrackedObject(int id) => Id = id;
    }

    // Short-lived object: Simulates a token buffer
    public class TransientToken : TrackedObject
    {
        public string Payload { get; set; }
        public TransientToken(int id, string payload) : base(id) 
        { 
            Payload = payload; 
        }
    }

    // Long-lived object: Simulates large model weights (approx 8KB, well below LOH threshold)
    // Note: For true LOH testing, we'd need >85k bytes, but for generational analysis 
    // within a standard loop, smaller objects are sufficient to demonstrate promotion.
    public class CachedModelWeights : TrackedObject
    {
        public double[] Weights { get; set; }
        public CachedModelWeights(int id) : base(id)
        {
            // Allocate ~8KB (1000 doubles * 8 bytes)
            Weights = new double[1000];
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Generational Analysis...");
            
            var modelCache = new List<CachedModelWeights>();
            TransientToken sampledTransient = null;
            CachedModelWeights sampledCached = null;

            // Force a cleanup before starting to establish a baseline
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            int gen0Before = GC.CollectionCount(0);
            int gen1Before = GC.CollectionCount(1);
            int gen2Before = GC.CollectionCount(2);

            for (int i = 0; i < 10_000; i++)
            {
                // 1. Create Transient Token (Short-lived)
                var token = new TransientToken(i, $"TokenData_{i}");
                
                // Sample the first token to check its generation later
                if (i == 0) sampledTransient = token;

                // 2. Create Cached Weights every 100 iterations (Long-lived)
                if (i % 100 == 0)
                {
                    var weights = new CachedModelWeights(i);
                    modelCache.Add(weights);
                    
                    // Sample the first cached weight
                    if (i == 0) sampledCached = weights;
                }

                // 3. Strategic Collections to force promotion
                // We collect every 500 iterations to force surviving objects to move up.
                if (i % 500 == 0 && i > 0)
                {
                    // Collect all generations
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: false);
                }
            }

            // Final collection to ensure all dead objects are cleared
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

            // Output Results
            Console.WriteLine("\n--- Results ---");
            Console.WriteLine($"Total Gen 0 Collections: {GC.CollectionCount(0) - gen0Before}");
            Console.WriteLine($"Total Gen 1 Collections: {GC.CollectionCount(1) - gen1Before}");
            Console.WriteLine($"Total Gen 2 Collections: {GC.CollectionCount(2) - gen2Before}");

            if (sampledTransient != null)
            {
                Console.WriteLine($"Sampled Transient Token (ID: {sampledTransient.Id}) Generation: {GC.GetGeneration(sampledTransient)}");
            }

            if (sampledCached != null)
            {
                Console.WriteLine($"Sampled Cached Model Weights (ID: {sampledCached.Id}) Generation: {GC.GetGeneration(sampledCached)}");
            }

            Console.WriteLine($"\nCached Objects Count: {modelCache.Count} (All should be in Gen 2)");
        }
    }
}
