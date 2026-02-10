
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

// ResilientLogProducer.cs
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client; // Assuming RabbitMQ.Client is used for the actual sending
using System.Text;

public class ResilientLogProducer
{
    private readonly Channel<string> _logChannel;
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;
    private readonly string _rabbitMqConnectionString;

    public ResilientLogProducer(string rabbitMqConnectionString)
    {
        _rabbitMqConnectionString = rabbitMqConnectionString;

        // Initialize Channel with BoundedCapacity options to prevent memory exhaustion
        _logChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait // Backpressure handling
        });

        // Initialize Circuit Breaker policy
        // Break after 3 consecutive failures, wait 30 seconds before trying again
        _circuitBreaker = Policy
            .Handle<Exception>() // Catch connection exceptions
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) => 
                    Console.WriteLine($"Circuit Broken! Waiting {breakDelay.TotalSeconds}s. Exception: {ex.Message}"),
                onReset: () => Console.WriteLine("Circuit Reset. Resuming normal operation.")
            );
    }

    public async Task QueueLogAsync(string logMessage)
    {
        // 1. Attempt to write to RabbitMQ via Circuit Breaker
        try
        {
            await _circuitBreaker.ExecuteAsync(async () => 
                await SendToRabbitMQAsync(logMessage)
            );
        }
        catch (BrokenCircuitException)
        {
            // Circuit is open, execution blocked. Fallback to channel buffering.
            await BufferToChannel(logMessage);
        }
        catch (Exception)
        {
            // Other exceptions during execution (if not breaking circuit yet)
            // Fallback to channel buffering
            await BufferToChannel(logMessage);
        }
    }

    private async Task BufferToChannel(string message)
    {
        // 2. If Circuit Breaker is open or execution fails, write to _logChannel
        Console.WriteLine("RabbitMQ unavailable. Buffering log to internal channel.");
        await _logChannel.Writer.WriteAsync(message);
    }

    private async Task SendToRabbitMQAsync(string message)
    {
        // Simulate sending to RabbitMQ
        // In production: create connection, channel, and publish
        if (string.IsNullOrEmpty(_rabbitMqConnectionString)) throw new InvalidOperationException("Connection string missing");
        
        // Simulate network delay
        await Task.Delay(50); 
        
        // Simulate random failure for demonstration
        if (new Random().Next(0, 10) < 2) 
            throw new Exception("Simulated network failure");

        Console.WriteLine($"Sent to RabbitMQ: {message}");
    }

    // 3. Implement a background loop to drain _logChannel when RabbitMQ recovers
    public async Task StartDrainingBufferAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in _logChannel.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                // Attempt to send buffered messages
                await _circuitBreaker.ExecuteAsync(async () => 
                    await SendToRabbitMQAsync(message)
                );
                // If successful, the message is dequeued automatically by the iterator
            }
            catch (Exception)
            {
                // If still failing, put it back (simple approach) or wait
                // For this demo, we just re-queue to the front of the channel
                await _logChannel.Writer.WriteAsync(message);
                
                // Wait a bit before retrying to avoid tight loop during outage
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
