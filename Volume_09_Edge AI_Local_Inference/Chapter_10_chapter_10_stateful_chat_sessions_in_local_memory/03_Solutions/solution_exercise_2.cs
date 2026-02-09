
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
using System.Linq;
using System.Text.RegularExpressions;

public static class TokenCounter
{
    // Basic heuristic: 1 token ~= 4 characters for English text
    // This is a rough approximation for BPE used in models like GPT.
    public static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        // Split by whitespace to isolate words
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        int tokenCount = 0;

        foreach (var word in words)
        {
            // Approximate tokens per word: Ceiling(length / 4.0)
            tokenCount += (int)Math.Ceiling(word.Length / 4.0);
        }

        // Add tokens for punctuation or special chars not caught in word splitting
        // Regex looks for standalone punctuation marks.
        var punctuation = Regex.Matches(text, @"[^\w\s]").Count;
        tokenCount += punctuation;

        return tokenCount;
    }

    public static string TruncatePrompt(string prompt, int maxTokens)
    {
        int currentTokens = EstimateTokens(prompt);

        if (currentTokens <= maxTokens)
        {
            return prompt;
        }

        // Strategy: Remove the middle, keep start and end (Ellipsis strategy)
        // We aim to keep roughly 40% start, 40% end, and 20% for ellipsis/overhead.
        
        var words = prompt.Split(' ');
        int totalWords = words.Length;
        
        // Calculate indices to keep
        int keepStart = (int)(totalWords * 0.4);
        int keepEnd = (int)(totalWords * 0.4);
        
        if (keepStart + keepEnd >= totalWords) return prompt; // Fallback

        var startPart = string.Join(" ", words.Take(keepStart));
        var endPart = string.Join(" ", words.Skip(totalWords - keepEnd));

        return $"{startPart} ... {endPart}";
    }
}

// Example usage within Exercise 1's GetContext
// var context = buffer.GetContext(100, TokenCounter.EstimateTokens);
