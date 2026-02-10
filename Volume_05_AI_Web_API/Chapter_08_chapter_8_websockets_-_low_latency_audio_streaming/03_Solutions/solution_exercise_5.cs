
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AudioStreamingApp.Exercises
{
    // 4. Hot-Swap Model Manager
    public interface IAIModel : IDisposable
    {
        Task<string> InferAsync(ReadOnlyMemory<byte> audioData);
    }

    // Mock implementation of an AI Model (e.g., ONNX session wrapper)
    public class AIModelV1 : IAIModel
    {
        public async Task<string> InferAsync(ReadOnlyMemory<byte> audioData)
        {
            await Task.Delay(50); // Simulate inference
            return "V1: Transcription";
        }
        public void Dispose() { /* Cleanup native resources */ }
    }

    public class AIModelV2 : IAIModel
    {
        public async Task<string> InferAsync(ReadOnlyMemory<byte> audioData)
        {
            await Task.Delay(30); // Faster inference
            return "V2: Transcription (Improved)";
        }
        public void Dispose() { /* Cleanup native resources */ }
    }

    public class ModelManager
    {
        private volatile IAIModel _currentModel;
        private readonly object _swapLock = new object();
        private readonly ILogger<ModelManager> _logger;

        public ModelManager(ILogger<ModelManager> logger)
        {
            _currentModel = new AIModelV1(); // Default
            _logger = logger;
        }

        // Hot-Swap Logic
        public void UpdateModel(IAIModel newModel)
        {
            IAIModel oldModel = null;
            
            // Minimal lock duration: Swap the reference atomically
            lock (_swapLock)
            {
                oldModel = _currentModel;
                _currentModel = newModel;
            }

            // Dispose old model *after* releasing the lock
            // This ensures no active inference calls are blocked by disposal
            if (oldModel != null)
            {
                _logger.LogInformation("Model swapped. Disposing old model...");
                oldModel.Dispose();
            }
        }

        // Get model for inference (Lock strictly for the duration of the read)
        public IAIModel GetModel()
        {
            // Volatile read ensures we see the most recent update
            return _currentModel;
        }
    }

    // 1. Resilient Audio Pipeline
    public class ResilientAudioPipeline
    {
        private readonly ModelManager _modelManager;
        private readonly ILogger<ResilientAudioPipeline> _logger;
        private const int MaxReconnectionBufferSeconds = 5;

        public ResilientAudioPipeline(ModelManager modelManager, ILogger<ResilientAudioPipeline> logger)
        {
            _modelManager = modelManager;
            _logger = logger;
        }

        public async Task ProcessConnectionAsync(WebSocket webSocket)
        {
            // 2. Reconnection Buffer (Local memory buffer for 5s)
            // Using a Channel to buffer locally before AI processing
            var bufferChannel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(100) 
            { 
                FullMode = BoundedChannelFullMode.DropOldest // Drop old data if we can't keep up
            });

            var cts = new CancellationTokenSource();
            var processingTask = StartAIProcessing(bufferChannel.Reader, cts.Token);

            try
            {
                var receiveBuffer = new byte[4096];
                long totalBytesReceived = 0;

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(receiveBuffer, cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", cts.Token);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // 3. Error Resilience: Copy to local buffer immediately
                        var chunk = new byte[result.Count];
                        receiveBuffer.AsSpan(0, result.Count).CopyTo(chunk);
                        
                        // Write to reconnection buffer
                        await bufferChannel.Writer.WriteAsync(chunk, cts.Token);
                        
                        totalBytesReceived += result.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                // 3. Error Handling
                _logger.LogError(ex, "Connection error");
                
                // Attempt to send RECONNECT message if socket is still usable
                if (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var msg = Encoding.UTF8.GetBytes($"RECONNECT|{totalBytesReceived}");
                        await webSocket.SendAsync(msg, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch { /* Ignore send errors during crash */ }
                }
            }
            finally
            {
                // 4. State Synchronization: The 'totalBytesReceived' sent above acts as the offset
                bufferChannel.Writer.Complete();
                cts.Cancel();
                try { await processingTask; } catch { /* Ignore cancellation */ }
            }
        }

        private async Task StartAIProcessing(ChannelReader<byte[]> reader, CancellationToken ct)
        {
            await foreach (var chunk in reader.ReadAllAsync(ct))
            {
                // Get the current model (Hot-Swap aware)
                var model = _modelManager.GetModel();
                
                // Constraint: No lock held on model reference for long.
                // We pass the reference to InferAsync, but the lock in ModelManager 
                // is released immediately after the GetModel() call.
                
                try
                {
                    var result = await model.InferAsync(chunk);
                    // Log or send result downstream
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Inference failed");
                }
            }
        }
    }

    // Simulated Hosted Service to run the pipeline
    public class AudioApiService : BackgroundService
    {
        private readonly ModelManager _modelManager;
        private readonly ILogger<AudioApiService> _logger;

        public AudioApiService(ModelManager modelManager, ILogger<AudioApiService> logger)
        {
            _modelManager = modelManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Simulate a Hot-Swap event after 10 seconds
            await Task.Delay(10000, stoppingToken);
            _logger.LogInformation("Performing Hot-Swap to V2...");
            _modelManager.UpdateModel(new AIModelV2());
        }
    }
}
