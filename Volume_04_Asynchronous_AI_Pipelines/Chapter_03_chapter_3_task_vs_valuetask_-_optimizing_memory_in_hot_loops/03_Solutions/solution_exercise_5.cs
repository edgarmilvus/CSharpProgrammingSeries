
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class LlmTokenGenerator : IAsyncEnumerable<string>
{
    private readonly Channel<string> _tokenChannel;

    public LlmTokenGenerator()
    {
        // Create an unbounded channel to handle backpressure via the consumer's speed
        _tokenChannel = Channel.CreateUnbounded<string>();
    }

    public async Task FeedTokenAsync(string token)
    {
        await _tokenChannel.Writer.WriteAsync(token);
    }

    public void Complete()
    {
        _tokenChannel.Writer.Complete();
    }

    public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new Enumerator(_tokenChannel.Reader, cancellationToken);
    }

    private class Enumerator : IAsyncEnumerator<string>
    {
        private readonly ChannelReader<string> _reader;
        private readonly CancellationToken _cancellationToken;
        
        public string Current { get; private set; } = string.Empty;

        public Enumerator(ChannelReader<string> reader, CancellationToken cancellationToken)
        {
            _reader = reader;
            _cancellationToken = cancellationToken;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            // Check cancellation
            if (_cancellationToken.IsCancellationRequested)
                return false;

            // WaitToReadAsync returns a ValueTask<bool>
            if (await _reader.WaitToReadAsync(_cancellationToken))
            {
                // TryRead returns bool. If true, we have data synchronously.
                if (_reader.TryRead(out string? token))
                {
                    Current = token;
                    return true;
                }
            }
            
            // Channel is closed and empty
            return false;
        }

        // Dispose is required by interface but often empty for simple channels
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}

// Usage Example
public class ConsumerDemo
{
    public static async Task Run()
    {
        var generator = new LlmTokenGenerator();

        // Start a producer task
        var producer = Task.Run(async () =>
        {
            await generator.FeedTokenAsync("Hello");
            await Task.Delay(100); // Simulate delay
            await generator.FeedTokenAsync("World");
            await Task.Delay(100);
            generator.Complete();
        });

        // Consume using await foreach
        await foreach (var token in generator)
        {
            Console.WriteLine($"Received: {token}");
        }

        await producer;
    }
}
