
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

namespace Book3_Chapter5_PracticalExercises
{
    public class Exercise5_Solution
    {
        public class DataPipeline
        {
            private float[] _dataBuffer;

            public DataPipeline(int size)
            {
                _dataBuffer = new float[size];
                for (int i = 0; i < size; i++) _dataBuffer[i] = i * 0.1f;
            }

            public void ProcessInChunks(int chunkSize)
            {
                // Wrap the array in a Memory<T> wrapper
                Memory<float> bufferMemory = _dataBuffer.AsMemory();

                for (int start = 0; start < bufferMemory.Length; start += chunkSize)
                {
                    int length = Math.Min(chunkSize, bufferMemory.Length - start);
                    
                    // ZERO ALLOCATION SLICING
                    // This does not copy the data; it just creates a view (offset + length)
                    Memory<float> chunk = bufferMemory.Slice(start, length);
                    
                    ProcessChunk(chunk);
                }
            }

            private void ProcessChunk(Memory<float> chunk)
            {
                // Span<T> is the stack-only counterpart to Memory<T>
                // It allows safe, high-performance manipulation of the slice
                Span<float> span = chunk.Span;
                
                // Example: Normalize the chunk (subtract mean)
                float sum = 0;
                for (int i = 0; i < span.Length; i++) sum += span[i];
                
                if (span.Length > 0)
                {
                    float mean = sum / span.Length;
                    for (int i = 0; i < span.Length; i++) 
                    {
                        span[i] -= mean;
                    }
                }
            }
        }
    }
}
