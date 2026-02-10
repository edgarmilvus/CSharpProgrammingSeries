
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit; // Assuming xUnit for testing

public enum ChatMessageRole { User, Assistant, System }

public record ChatMessage(ChatMessageRole Role, string Content, DateTime Timestamp);

public class ConcurrentConversationBuffer
{
    private readonly ConcurrentQueue<ChatMessage> _messages = new();

    public void AddMessage(ChatMessageRole role, string content)
    {
        var message = new ChatMessage(role, content, DateTime.UtcNow);
        _messages.Enqueue(message);
    }

    public IEnumerable<ChatMessage> GetContext(int maxTokenLimit, Func<string, int> tokenCounter)
    {
        var context = new List<ChatMessage>();
        int currentTokenCount = 0;

        // ConcurrentQueue does not guarantee order when iterating, 
        // but ToArray() provides a snapshot.
        foreach (var message in _messages.ToArray())
        {
            int messageTokens = tokenCounter(message.Content);
            
            // Check if adding this message exceeds the limit
            if (currentTokenCount + messageTokens > maxTokenLimit)
            {
                break; 
            }

            context.Add(message);
            currentTokenCount += messageTokens;
        }

        return context;
    }

    public void Clear()
    {
        while (!_messages.IsEmpty)
        {
            _messages.TryDequeue(out _);
        }
    }

    public int Count => _messages.Count;
}

// Unit Test
public class ConversationBufferTests
{
    [Fact]
    public async Task Concurrent_Adds_Messages_Correctly()
    {
        // Arrange
        var buffer = new ConcurrentConversationBuffer();
        int messagesPerThread = 10;
        int threadCount = 5;

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < messagesPerThread; i++)
            {
                buffer.AddMessage(ChatMessageRole.User, $"Message {i}");
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(threadCount * messagesPerThread, buffer.Count);
    }
}
