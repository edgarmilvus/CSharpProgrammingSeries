
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
using System.Threading;
using System.Threading.Tasks;

public record ApiLogEntry(Guid RequestId, DateTime Timestamp, int TokenCount);

public class InfiniteStreamMonitor
{
    // 2. Implement IAsyncEnumerable with timeout
    public static async IAsyncEnumerable<ApiLogEntry> MonitorApiTrafficAsync(TimeSpan timeout)
    {
        // 3. Use a local CancellationTokenSource tied to the timeout
        using var cts = new CancellationTokenSource(timeout);
        var token = cts.Token;

        // 5. State Management: ConfigureAwait(false) to avoid deadlocks in sync contexts
        // (Important for library code or background services)
        
        try
        {
            while (true)
            {
                // 4. Check for termination condition (Timeout)
                token.ThrowIfCancellationRequested();

                // Simulate waiting for a log event
                // We use Task.Delay with the token. If timeout hits, this throws.
                await Task.Delay(TimeSpan.FromMilliseconds(200), token).ConfigureAwait(false);

                // Generate Log Entry
                var entry = new ApiLogEntry(
                    RequestId: Guid.NewGuid(),
                    Timestamp: DateTime.UtcNow,
                    TokenCount: new Random().Next(10, 100)
                );

                // 4. Yield the entry
                yield return entry;
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout reached. The loop breaks naturally, ending the stream.
            Console.WriteLine("Monitor: Timeout elapsed. Stopping stream.");
        }
    }

    // 7. Consumer Implementation
    public static async Task RunConsumerAsync()
    {
        // Set a 2-second timeout for the stream
        var timeout = TimeSpan.FromSeconds(2);
        
        try
        {
            int itemsProcessed = 0;
            
            // The consumer loop requests items. 
            // The iterator internally awaits Task.Delay. 
            // When the timeout CTS triggers, the iterator throws OCE, 
            // breaking the loop gracefully.
            await foreach (var entry in MonitorApiTrafficAsync(timeout))
            {
                itemsProcessed++;
                Console.WriteLine($"[{itemsProcessed}] Req: {entry.RequestId}, Tokens: {entry.TokenCount}");

                // 7. Stop after 100 entries or stream end
                if (itemsProcessed >= 100)
                {
                    Console.WriteLine("Consumer: Reached max limit of 100 entries.");
                    break; 
                }
            }
        }
        catch (OperationCanceledException)
        {
            // This catches the cancellation if it bubbles up (though our iterator handles it internally)
            Console.WriteLine("Consumer: Stream operation was cancelled.");
        }
    }
}

// Entry point
// await InfiniteStreamMonitor.RunConsumerAsync();
