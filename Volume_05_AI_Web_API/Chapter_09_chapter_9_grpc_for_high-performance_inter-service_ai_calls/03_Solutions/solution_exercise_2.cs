
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

// File: Services/TranscriptionService.cs
using Grpc.Core;
using System;
using System.Threading.Tasks;

// Assuming generated classes from transcription.proto
namespace TranscriptionServices
{
    public class TranscriptionServiceImpl : TranscriptionService.TranscriptionServiceBase
    {
        public override async Task StreamTranscribe(
            IAsyncStreamReader<AudioChunk> requestStream, 
            IServerStreamWriter<TranscriptionSegment> responseStream, 
            ServerCallContext context)
        {
            // 1. Move to the first request (client must send something to start)
            // This await is necessary to establish the stream context.
            await foreach (var chunk in requestStream.ReadAllAsync())
            {
                // 2. Simulate processing logic
                // In a real scenario, we would feed 'chunk.Data' to an audio processing model.
                // Here we simulate accumulating bytes until a "silence" (end of sentence).
                
                // Simulate silence detection logic
                bool isFinal = chunk.SequenceId % 10 == 0; // Every 10th chunk is "final"

                if (isFinal)
                {
                    var segment = new TranscriptionSegment
                    {
                        Text = $"Processed audio chunk {chunk.SequenceId}",
                        IsFinal = true
                    };

                    // 3. Write to the response stream
                    await responseStream.WriteAsync(segment);
                }
            }

            // 4. Edge Case Handling (Backpressure)
            // BACKPRESSURE IN gRPC:
            // gRPC handles backpressure automatically via TCP flow control.
            // If the client writes faster than the server reads, the client's 'WriteAsync' 
            // will eventually await (block) until the server consumes the buffer.
            // However, if the server writes faster than the client reads, the server's 
            // 'WriteAsync' will await until the client acknowledges data.
            // 
            // To prevent server memory overflow from a fast sender:
            // - Use 'await foreach' on the requestStream as shown above. 
            // - This processes items sequentially. 
            // - If the server needs to limit concurrent processing, use a SemaphoreSlim 
            //   around the processing logic inside the loop.
        }
    }
}
