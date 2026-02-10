
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Threading.Tasks;

public class StreamProcessor
{
    // 3. Async Context: We cannot use Span directly in async state machines.
    // However, if the processing logic is synchronous (just the slicing is sync),
    // we can iterate and call async methods by passing data.
    // Since Action<ReadOnlyMemory<int>> is passed in, we can invoke it.
    
    public static async Task ProcessStream(ReadOnlyMemory<int> stream, int chunkSize, Func<ReadOnlyMemory<int>, Task> processChunk)
    {
        int start = 0;
        while (start < stream.Length)
        {
            int remaining = stream.Length - start;
            int currentSize = Math.Min(chunkSize, remaining);

            // 2. Zero-Copy Slicing
            ReadOnlyMemory<int> chunk = stream.Slice(start, currentSize);

            // We pass the Memory<int> to the processor.
            // Inside the processor, it will convert to Span<int> to read data.
            await processChunk(chunk);

            start += currentSize;
        }
    }

    // Helper to demonstrate usage
    public static async Task Demonstrate()
    {
        int[] data = new int[1000];
        for (int i = 0; i < data.Length; i++) data[i] = i;

        // Process in chunks of 128
        await ProcessStream(data, 128, async chunkMemory =>
        {
            // To read the data, we must use Span.
            // Since this lambda is not an iterator and we aren't using unsafe context,
            // we can convert Memory to Span.
            ReadOnlySpan<int> chunkSpan = chunkMemory.Span;

            // Simulate async work (e.g., sending to model)
            await Task.Delay(10); 
            
            // Read data (e.g., sum)
            int sum = 0;
            foreach (var val in chunkSpan) sum += val;
            Console.WriteLine($"Processed chunk sum: {sum}");
        });
    }
}

// Interactive Challenge: Parallel Processing with Pinning
public class ParallelStreamProcessor
{
    public static void ProcessParallel(ReadOnlyMemory<int> stream, int chunkSize, Action<ReadOnlySpan<int>> processChunk)
    {
        // We cannot easily use Task Parallel Library (TPL) with Span because Span is stack-only.
        // However, we can use threads and pinning if we own the underlying array.
        
        // If the stream is backed by an array, we can get the handle.
        if (!MemoryMarshal.TryGetArray(stream, out ArraySegment<int> segment))
        {
            throw new InvalidOperationException("Cannot access underlying array for pinning.");
        }

        int[] array = segment.Array;
        int offset = segment.Offset;
        int length = segment.Count;

        // Calculate number of chunks
        int chunks = (int)Math.Ceiling((double)length / chunkSize);
        var tasks = new Task[chunks];

        for (int i = 0; i < chunks; i++)
        {
            int start = offset + (i * chunkSize);
            int end = Math.Min(start + chunkSize, offset + length);
            int size = end - start;

            // Capture variables for closure
            int localStart = start;
            int localSize = size;

            tasks[i] = Task.Run(() =>
            {
                // Create a Span over the specific part of the array.
                // This is safe because we are on a single thread within the task,
                // and the array is pinned by the GC (unless we manually pin).
                
                // To allow true parallel access without GC moving the memory, 
                // we could use 'fixed' or 'GCHandle'.
                
                unsafe
                {
                    fixed (int* ptr = &array[localStart])
                    {
                        // Create a Span from the pointer
                        Span<int> slice = new Span<int>(ptr, localSize);
                        
                        // We can pass this Span to the processor.
                        // Note: Span<T> is safe to use on the stack within a single thread.
                        processChunk(slice);
                    }
                }
            });
        }

        Task.WaitAll(tasks);
    }
}
