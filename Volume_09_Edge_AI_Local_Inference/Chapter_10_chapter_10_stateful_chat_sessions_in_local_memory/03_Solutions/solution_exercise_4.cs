
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class CriticalContextOverflowException : Exception
{
    public CriticalContextOverflowException(string message) : base(message) { }
}

public class SlidingWindowBuffer : ConcurrentConversationBuffer
{
    // Cache for token counts to avoid re-calculation
    private readonly Dictionary<string, int> _tokenCache = new();

    public new IEnumerable<ChatMessage> GetContext(int maxTokenLimit, Func<string, int> tokenCounter)
    {
        var allMessages = _messages.ToArray();
        if (allMessages.Length == 0) return Enumerable.Empty<ChatMessage>();

        // 1. Edge Case: Check System Prompt
        var systemMsg = allMessages.FirstOrDefault(m => m.Role == ChatMessageRole.System);
        if (systemMsg != null)
        {
            int sysTokens = GetCachedTokens(systemMsg.Content, tokenCounter);
            if (sysTokens > maxTokenLimit)
            {
                throw new CriticalContextOverflowException("System prompt exceeds maximum token limit.");
            }
        }

        // 2. Sliding Window Logic
        var result = new List<ChatMessage>();
        int currentTokens = 0;

        // Always keep the first message (assuming it's System or initial context)
        // In a real scenario, we might separate System/Initial User strictly.
        
        // Strategy: 
        // A. Keep System Prompt (if exists)
        // B. Keep Last N messages (Recent)
        // C. Fill remaining space with Middle if possible, otherwise prioritize Recent.

        // Simplified Logic for this exercise:
        // 1. Add System Prompt (always)
        // 2. Iterate backwards from the end (Newest) to fill budget.
        // 3. If budget remains, iterate forwards (Oldest) skipping system.

        // Step A: Add System Prompt
        if (systemMsg != null)
        {
            int sysTokens = GetCachedTokens(systemMsg.Content, tokenCounter);
            result.Add(systemMsg);
            currentTokens += sysTokens;
        }

        // Step B: Add Recent Messages (Reverse Iteration)
        // Filter out system message to avoid duplication
        var nonSystemMessages = allMessages.Where(m => m.Role != ChatMessageRole.System).ToArray();
        
        for (int i = nonSystemMessages.Length - 1; i >= 0; i--)
        {
            var msg = nonSystemMessages[i];
            int msgTokens = GetCachedTokens(msg.Content, tokenCounter);

            if (currentTokens + msgTokens <= maxTokenLimit)
            {
                result.Add(msg);
                currentTokens += msgTokens;
            }
            else
            {
                break; // Stop adding recent messages if limit hit
            }
        }

        // Note: In a full sliding window, we might want to preserve order. 
        // The result list is currently [System, Recent..., Oldest...].
        // For display purposes, we should sort by Timestamp.
        return result.OrderBy(m => m.Timestamp);
    }

    private int GetCachedTokens(string content, Func<string, int> tokenCounter)
    {
        if (!_tokenCache.TryGetValue(content, out int tokens))
        {
            tokens = tokenCounter(content);
            _tokenCache[content] = tokens;
        }
        return tokens;
    }
}
