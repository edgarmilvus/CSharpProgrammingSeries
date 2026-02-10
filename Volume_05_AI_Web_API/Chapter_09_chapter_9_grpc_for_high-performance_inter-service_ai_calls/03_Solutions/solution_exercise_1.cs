
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

// File: Services/InferenceService.cs
using Grpc.Core;
using AiServices.Inference.V1; // Assuming namespace from Exercise 1
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiServices.Services
{
    public class InferenceServiceImpl : InferenceService.InferenceServiceBase
    {
        private readonly Random _random = new Random();

        // Server-side streaming RPC
        public override async Task GenerateStream(
            InferenceRequest request, 
            IServerStreamWriter<InferenceResponse> responseStream, 
            ServerCallContext context)
        {
            // CRITICAL: Extract the CancellationToken from the context.
            // This token is triggered automatically if the client disconnects or cancels.
            var cancellationToken = context.CancellationToken;

            // Simulate generating tokens one by one
            string[] tokens = { "The", " quick", " brown", " fox", " jumps", " over", " the", " lazy", " dog." };
            
            try
            {
                foreach (var token in tokens)
                {
                    // Check for cancellation before doing work
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Log or handle cancellation gracefully
                        Console.WriteLine("Client cancelled the stream. Stopping generation.");
                        break;
                    }

                    // Simulate LLM processing delay
                    await Task.Delay(_random.Next(50, 200), cancellationToken);

                    // Create the response chunk
                    var response = new InferenceResponse
                    {
                        GeneratedText = token,
                        ConfidenceScore = (float)_random.NextDouble(),
                        // In a stream, stop_reasons might only be sent on the final chunk
                        StopReasons = { } 
                    };

                    // Write to the response stream
                    await responseStream.WriteAsync(response, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Handle the specific exception thrown by Task.Delay or WriteAsync when cancelled
                Console.WriteLine("Operation was cancelled during streaming.");
            }
        }
    }
}
