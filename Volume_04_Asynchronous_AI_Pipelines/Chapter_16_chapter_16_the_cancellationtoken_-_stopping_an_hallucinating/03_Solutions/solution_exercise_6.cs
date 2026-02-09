
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

# Source File: solution_exercise_6.cs
# Description: Solution for Exercise 6
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class DataflowIntegrationExercise
{
    public async Task<BufferBlock<string>> StreamToDataflowAsync(
        IAsyncEnumerable<string> source, 
        CancellationToken cancellationToken)
    {
        var buffer = new BufferBlock<string>(new DataflowBlockOptions 
        { 
            BoundedCapacity = 10 
        });

        // Run producer in a separate task to avoid blocking the caller
        var producer = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in source)
                {
                    // Try to send item to buffer with cancellationToken
                    // SendAsync respects the token; if canceled, it returns false or throws.
                    bool accepted = await buffer.SendAsync(item, cancellationToken);
                    
                    if (!accepted)
                    {
                        // Buffer is full or cancellation requested
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected if token is canceled during SendAsync or enumeration
            }
            finally
            {
                // Ensure we complete the block even on cancellation
                // This signals the consumer that no more data is coming.
                buffer.Complete();
            }
        }, cancellationToken);

        // Return the buffer immediately. 
        // Note: We don't await the producer task here; the buffer is returned 
        // for the consumer to read from while the producer writes.
        return buffer;
    }

    public async Task RunExercise()
    {
        // Create a simple infinite async enumerable
        async IAsyncEnumerable<string> InfiniteSource(CancellationToken ct)
        {
            int i = 0;
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(50, ct);
                yield return $"Item_{i++}";
            }
        }

        using var cts = new CancellationTokenSource(300); // Cancel after 300ms
        
        var source = InfiniteSource(cts.Token);
        var buffer = await StreamToDataflowAsync(source, cts.Token);

        // Consume from buffer
        int count = 0;
        while (await buffer.OutputAvailableAsync())
        {
            var item = await buffer.ReceiveAsync();
            Console.WriteLine($"Received: {item}");
            count++;
        }
        
        Console.WriteLine($"Buffer completed. Received {count} items.");
    }
}
