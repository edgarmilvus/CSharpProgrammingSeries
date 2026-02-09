
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

// PROBLEM CONTEXT: Real-time AI Sentiment Analysis Pipeline
// A customer support platform receives thousands of chat messages per minute. 
// Each message must be analyzed for sentiment (Positive, Negative, Neutral) 
// to prioritize urgent issues. The system must handle backpressure when 
// the AI model inference slows down, preventing memory overflow.

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== AI Sentiment Analysis Pipeline Started ===\n");

        // 1. CHANNEL CREATION (Bounded Capacity)
        // We use a bounded channel to limit memory usage. If the channel is full,
        // the producer will wait (backpressure) instead of crashing the system.
        var chatChannel = Channel.CreateBounded<ChatMessage>(new BoundedChannelOptions(capacity: 10)
        {
            FullMode = BoundedChannelFullMode.Wait // Blocks producer when full
        });

        // 2. CANCELLATION TOKEN SOURCE
        // Allows graceful shutdown of the pipeline on Ctrl+C.
        var cts = new CancellationTokenSource();

        // 3. PRODUCER TASK (Simulating Incoming Chat Stream)
        // This simulates a high-throughput stream of user messages.
        Task producerTask = Task.Run(async () =>
        {
            int messageId = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    // Simulate random arrival of messages
                    await Task.Delay(500, cts.Token); 
                    
                    var message = new ChatMessage
                    {
                        Id = ++messageId,
                        Text = GetMessageContent(messageId),
                        Timestamp = DateTime.UtcNow
                    };

                    // Write to channel. If channel is full, this awaits until space is available.
                    await chatChannel.Writer.WriteAsync(message, cts.Token);
                    Console.WriteLine($"[Producer] Enqueued Message ID: {message.Id}");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            // Signal that no more items will be written.
            chatChannel.Writer.Complete();
            Console.WriteLine("[Producer] Finished sending messages.");
        });

        // 4. CONSUMER TASKS (Parallel AI Inference Workers)
        // We spawn multiple consumers to simulate parallel processing 
        // (e.g., multiple GPU threads or distributed workers).
        int consumerCount = 3;
        var consumerTasks = new Task[consumerCount];

        for (int i = 0; i < consumerCount; i++)
        {
            int workerId = i + 1;
            consumerTasks[i] = Task.Run(async () =>
            {
                // Read from channel until it's marked as complete and empty.
                await foreach (var message in chatChannel.Reader.ReadAllAsync(cts.Token))
                {
                    try
                    {
                        // Simulate AI Model Inference latency
                        await Task.Delay(new Random().Next(800, 1500), cts.Token);

                        // Process the message (Mock AI Logic)
                        var sentiment = AnalyzeSentiment(message.Text);
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[Worker {workerId}] Processed Msg {message.Id}: {sentiment}");
                        Console.ResetColor();
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Worker {workerId}] Error: {ex.Message}");
                    }
                }
                Console.WriteLine($"[Worker {workerId}] Stopped.");
            });
        }

        // 5. CONTROL FLOW (Graceful Shutdown)
        Console.WriteLine("Press Ctrl+C to stop the pipeline...");
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        // Wait for producer to finish (it will finish when Ctrl+C is pressed or logic completes)
        await producerTask;
        
        // Wait for consumers to drain the channel
        await Task.WhenAll(consumerTasks);

        Console.WriteLine("\n=== Pipeline Shutdown Complete ===");
    }

    // --- Helper Methods (Basic Logic) ---

    static string GetMessageContent(int id)
    {
        string[] templates = {
            "I love this product, it works perfectly!",
            "This is terrible, I want a refund immediately.",
            "Can you help me reset my password?",
            "The update is okay, but not great.",
            "Disgusting service, never buying again."
        };
        return templates[id % templates.Length];
    }

    static string AnalyzeSentiment(string text)
    {
        // Mock AI Model Logic
        if (text.Contains("love") || text.Contains("perfectly")) return "POSITIVE";
        if (text.Contains("terrible") || text.Contains("refund") || text.Contains("Disgusting")) return "NEGATIVE";
        return "NEUTRAL";
    }
}

// --- Data Model ---
// Simple class to hold message data.
public class ChatMessage
{
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
}
