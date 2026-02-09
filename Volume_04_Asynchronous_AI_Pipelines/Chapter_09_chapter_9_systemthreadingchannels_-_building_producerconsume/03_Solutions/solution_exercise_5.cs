
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

public class DynamicWorkerScalingPipeline
{
    // Configuration
    private const int ChannelCapacity = 100;
    private const int WorkerCount = 4;

    public static async Task RunAsync()
    {
        // 1. Bounded Channel with Wait mode for backpressure
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        // 2. Start the Producer
        var producerTask = ProducerAsync(channel.Writer);

        // 3. Start the Worker Pool (Parallel Consumers)
        var workerTasks = new List<Task>();
        for (int i = 0; i < WorkerCount; i++)
        {
            workerTasks.Add(WorkerAsync(i, channel.Reader));
        }

        // 4. Graceful Shutdown Logic
        // Wait for producer to finish sending data
        await producerTask;
        
        // Signal the channel that no more data is coming
        channel.Writer.Complete();

        // Wait for all workers to finish processing remaining data
        await Task.WhenAll(workerTasks);

        Console.WriteLine("Pipeline refactored and finished successfully.");
    }

    private static async Task ProducerAsync(ChannelWriter<string> writer)
    {
        // Simulate high-throughput data generation
        for (int i = 1; i <= 500; i++)
        {
            // 3. Backpressure Handling
            // Because FullMode is Wait, this await will pause if the workers 
            // cannot keep up with the 500 items.
            await writer.WriteAsync($"Data_{i}");
            
            if (i % 50 == 0) Console.WriteLine($"[Producer] Sent {i} items...");
        }
    }

    private static async Task WorkerAsync(int id, ChannelReader<string> reader)
    {
        Console.WriteLine($"Worker {id} started.");
        
        // Read loop handles the Completion signal automatically
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var item))
            {
                // 5. Refactoring Constraint: Core logic unchanged (simulated AI work)
                await SimulateAIWork(item);
            }
        }
        Console.WriteLine($"Worker {id} stopped.");
    }

    // The "Core" logic simulation
    private static async Task SimulateAIWork(string item)
    {
        // Simulate variable processing time
        await Task.Delay(50); 
        // In a real scenario, we might log or store the result here
    }
}
