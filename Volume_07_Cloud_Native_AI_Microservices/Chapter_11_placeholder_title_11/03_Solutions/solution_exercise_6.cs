
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

// Source File: solution_exercise_6.cs
// Description: Solution for Exercise 6
// ==========================================

using System.Threading.Channels;

namespace InferencePipeline
{
    public record ImageRequest(string ImageId, byte[] ImageData, CancellationToken CancellationToken);

    public class InferencePipeline
    {
        private readonly Channel<ImageRequest> _channel;
        private readonly int _maxConcurrency;

        public InferencePipeline(int maxConcurrency = 4)
        {
            _maxConcurrency = maxConcurrency;
            
            // Create a bounded channel to handle backpressure
            _channel = Channel.CreateBounded<ImageRequest>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        // Entry point for the controller
        public async Task SubmitForProcessingAsync(ImageRequest request)
        {
            // Write to the channel (non-blocking)
            await _channel.Writer.WriteAsync(request, request.CancellationToken);
        }

        // Background worker method (to be started in Program.cs)
        public async Task StartProcessingLoopAsync()
        {
            // Create a pool of workers limited by concurrency
            var tasks = Enumerable.Range(0, _maxConcurrency)
                .Select(_ => ProcessWorkerAsync())
                .ToArray();

            await Task.WhenAll(tasks);
        }

        private async Task ProcessWorkerAsync()
        {
            // Read from the channel continuously
            await foreach (var request in _channel.Reader.ReadAllAsync(request.CancellationToken))
            {
                try
                {
                    // 1. Preprocessing (CPU Bound)
                    var preprocessedData = await Task.Run(() => 
                        Preprocess(request.ImageData), 
                        request.CancellationToken
                    );

                    // 2. Inference (I/O/CPU Bound - Simulated)
                    var inferenceResult = await RunInferenceAsync(preprocessedData, request.CancellationToken);

                    // 3. Postprocessing (CPU Bound)
                    var finalResult = await Task.Run(() =>
                        Postprocess(inferenceResult),
                        request.CancellationToken
                    );

                    // 4. Return result (omitted for brevity)
                }
                catch (OperationCanceledException)
                {
                    // Handle client disconnect gracefully
                    Console.WriteLine($"Request {request.ImageId} cancelled.");
                }
            }
        }

        // Helper methods (Simulated)
        private byte[] Preprocess(byte[] data) { /* CPU work */ return data; }
        private async Task<byte[]> RunInferenceAsync(byte[] data, CancellationToken ct) 
        { 
            await Task.Delay(500, ct); // Simulate GPU/Network latency
            return data; 
        }
        private byte[] Postprocess(byte[] data) { /* CPU work */ return data; }
    }
}
