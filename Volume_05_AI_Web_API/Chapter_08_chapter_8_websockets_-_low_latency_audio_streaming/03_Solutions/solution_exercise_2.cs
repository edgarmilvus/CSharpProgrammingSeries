
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

using System;
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
    // 1. Define AudioChunk
    public record struct AudioChunk(ReadOnlyMemory<byte> Data, DateTime Timestamp);

    // 2. WebSocket Audio Producer
    public class WebSocketAudioProducer
    {
        private readonly ILogger<WebSocketAudioProducer> _logger;

        public WebSocketAudioProducer(ILogger<WebSocketAudioProducer> logger)
        {
            _logger = logger;
        }

        public async Task ProduceAsync(WebSocket socket, ChannelWriter<AudioChunk> writer, CancellationToken ct)
        {
            var buffer = new byte[4096]; // Smaller chunks for aggregation testing
            try
            {
                while (!ct.IsCancellationRequested && socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", ct);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // Copy to Memory (Producer side responsibility)
                        var memory = new byte[result.Count];
                        buffer.AsMemory(0, result.Count).CopyTo(memory);
                        
                        // Write to channel (Backpressure handled by channel bounds or blocking)
                        await writer.WriteAsync(new AudioChunk(memory, DateTime.UtcNow), ct);
                    }
                }
            }
            catch (OperationCanceledException) { /* Normal shutdown */ }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Producer error");
            }
            finally
            {
                // Signal consumer that no more data is coming
                writer.Complete();
            }
        }
    }

    // 4. AI Model Consumer (Background Service)
    public class AIModelConsumer : BackgroundService
    {
        private readonly ChannelReader<AudioChunk> _reader;
        private readonly ILogger<AIModelConsumer> _logger;
        private const int TargetSampleRate = 16000; // 16kHz
        private const int BytesPerSample = 2; // 16-bit audio
        private const int MinDurationMs = 500;

        public AIModelConsumer(ChannelReader<AudioChunk> reader, ILogger<AIModelConsumer> logger)
        {
            _reader = reader;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Interactive Challenge: Dynamic Chunk Aggregation
            var accumulatedBuffers = new List<byte[]>();
            int accumulatedBytes = 0;
            DateTime startTime = default;

            try
            {
                await foreach (var chunk in _reader.ReadAllAsync(stoppingToken))
                {
                    if (startTime == default) startTime = chunk.Timestamp;

                    // Add chunk to buffer
                    accumulatedBuffers.Add(chunk.Data.ToArray());
                    accumulatedBytes += chunk.Data.Length;

                    // Calculate duration in ms: (Bytes / (SampleRate * BytesPerSample)) * 1000
                    double durationMs = (accumulatedBytes / (double)(TargetSampleRate * BytesPerSample)) * 1000;

                    // Check Silence Threshold (Simulated: if chunk is tiny, treat as silence/pause)
                    bool silenceDetected = chunk.Data.Length < 10; 

                    // Process if duration > 500ms OR silence detected (flush)
                    if (durationMs >= MinDurationMs || silenceDetected)
                    {
                        if (accumulatedBytes > 0)
                        {
                            await ProcessAggregatedAudio(accumulatedBuffers, accumulatedBytes, stoppingToken);
                            
                            // Reset buffer
                            accumulatedBuffers.Clear();
                            accumulatedBytes = 0;
                            startTime = default;
                        }
                    }
                }
            }
            catch (OperationCanceledException) { /* Graceful stop */ }
        }

        private async Task ProcessAggregatedAudio(List<byte[]> buffers, int totalBytes, CancellationToken ct)
        {
            // Simulate AI Inference
            _logger.LogInformation($"Processing AI Inference on {totalBytes} bytes ({buffers.Count} chunks)");
            await Task.Delay(100, ct); // Simulate inference time
            
            // Output "Transcription"
            _logger.LogInformation("AI Output: \"...transcribed text...\"");
        }
    }

    // 5. Edge Case Handling (Drain on Disconnect)
    public class WebSocketManagerService
    {
        private readonly ILogger<WebSocketManagerService> _logger;

        public WebSocketManagerService(ILogger<WebSocketManagerService> logger)
        {
            _logger = logger;
        }

        public async Task HandleConnectionAsync(WebSocket socket, bool drainOnDisconnect)
        {
            // Create the channel with a bounded capacity to handle backpressure
            var channel = Channel.CreateBounded<AudioChunk>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait // Block producer if consumer is slow
            });

            var producer = new WebSocketAudioProducer(_logger);
            var consumer = new AIModelConsumer(channel.Reader, _logger);

            // Start Consumer (Background Service logic simulated here for single connection scope)
            var consumerTask = consumer.StartAsync(CancellationToken.None);

            // Start Producer
            var cts = new CancellationTokenSource();
            var producerTask = producer.ProduceAsync(socket, channel.Writer, cts.Token);

            // Wait for producer to finish (connection close)
            await producerTask;

            if (drainOnDisconnect)
            {
                _logger.LogInformation("Draining channel...");
                // Wait for channel to empty
                await channel.Reader.Completion;
            }
            else
            {
                _logger.LogInformation("Discarding remaining items...");
                channel.Writer.Complete(); // Stop accepting new items
            }

            cts.Cancel();
            await consumer.StopAsync(CancellationToken.None);
        }
    }
}
