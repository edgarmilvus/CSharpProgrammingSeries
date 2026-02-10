
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime; // Assuming usage for GPU detection
using Microsoft.ML.OnnxRuntime.Tensors;

namespace InferenceService
{
    // 2. Batching Queue Service using System.Threading.Channels
    public class InferenceBatchingService : BackgroundService
    {
        private readonly Channel<InferenceRequest> _channel;
        private readonly ILogger<InferenceBatchingService> _logger;
        private readonly int _maxBatchSize = 32;
        private readonly TimeSpan _batchTimeout = TimeSpan.FromMilliseconds(50);

        public InferenceBatchingService(ILogger<InferenceBatchingService> logger)
        {
            _channel = Channel.CreateUnbounded<InferenceRequest>();
            _logger = logger;
        }

        public async Task<bool> QueueRequestAsync(InferenceRequest request, CancellationToken ct)
        {
            // Propagate cancellation by linking the request'sCTS with the channel writer
            return await _channel.Writer.WriteAsync(request, ct);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Inference Batch Consumer started.");
            
            // 5. GPU Detection Logic
            var providers = OrtEnv.Instance.GetAvailableProviders();
            _logger.LogInformation("Available ONNX Runtime Providers: {Providers}", string.Join(", ", providers));
            
            // Select GPU if available, otherwise CPU
            var executionProvider = providers.Contains("CUDAExecutionProvider") ? "CUDAExecutionProvider" : "CPUExecutionProvider";
            _logger.LogInformation("Selected Execution Provider: {Provider}", executionProvider);

            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = new List<InferenceRequest>();
                
                try
                {
                    // Wait for the first item
                    if (await _channel.Reader.WaitToReadAsync(stoppingToken))
                    {
                        // Try to fill the batch until timeout or max size
                        var timeoutCts = new CancellationTokenSource(_batchTimeout);
                        while (batch.Count < _maxBatchSize)
                        {
                            if (_channel.Reader.TryRead(out var item))
                            {
                                batch.Add(item);
                            }
                            else
                            {
                                // Wait for next item or timeout
                                try
                                {
                                    await _channel.Reader.WaitToReadAsync(timeoutCts.Token);
                                }
                                catch (OperationCanceledException)
                                {
                                    // Timeout reached, process what we have
                                    break;
                                }
                            }
                        }

                        if (batch.Count > 0)
                        {
                            await ProcessBatchAsync(batch, executionProvider, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in batch consumer loop.");
                }
            }
        }

        private async Task ProcessBatchAsync(List<InferenceRequest> batch, string provider, CancellationToken ct)
        {
            _logger.LogInformation("Processing batch of {Count} requests", batch.Count);

            try
            {
                // 3. Mock Inference Execution
                await RunInferenceBatchAsync(batch, provider, ct);

                // Set results
                foreach (var req in batch)
                {
                    req.CompletionSource.TrySetResult($"Processed batch with {provider}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch processing failed");
                foreach (var req in batch)
                {
                    req.CompletionSource.TrySetException(ex);
                }
            }
        }

        private async Task RunInferenceBatchAsync(List<InferenceRequest> batch, string provider, CancellationToken ct)
        {
            // Simulate GPU computation delay
            // Real implementation would load ONNX model here and run inference
            await Task.Delay(20, ct); 
        }
    }

    public class InferenceRequest
    {
        public string Input { get; set; }
        public TaskCompletionSource<string> CompletionSource { get; } = new TaskCompletionSource<string>();
    }

    // 1. ASP.NET Core Controller
    [ApiController]
    [Route("api/v1/infer")]
    public class InferenceController : ControllerBase
    {
        private readonly InferenceBatchingService _batchingService;
        private readonly ILogger<InferenceController> _logger;

        public InferenceController(InferenceBatchingService batchingService, ILogger<InferenceController> logger)
        {
            _batchingService = batchingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Infer([FromBody] string input, CancellationToken cancellationToken)
        {
            var request = new InferenceRequest { Input = input };

            // 4. Propagate Cancellation
            // We create a linked token source to handle the specific request cancellation
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            try
            {
                // Queue the request
                await _batchingService.QueueRequestAsync(request, linkedCts.Token);

                // Wait for the result from the background consumer
                var result = await request.CompletionSource.Task;
                return Ok(new { result });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Inference request cancelled.");
                return StatusCode(499, "Client Closed Request"); // 499 is a standard code for client disconnection
            }
        }
    }
}
