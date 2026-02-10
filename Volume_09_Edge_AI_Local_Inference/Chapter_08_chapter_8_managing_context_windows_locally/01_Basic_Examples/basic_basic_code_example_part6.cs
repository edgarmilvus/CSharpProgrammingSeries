
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

// Source File: basic_basic_code_example_part6.cs
// Description: Basic Code Example
// ==========================================

public List<ConversationMessage> GetOptimizedContext(string currentQuery)
{
    // 1. Check if we even need to prune
    int currentTokens = _history.Sum(m => m.Content.Length / 4);
    if (currentTokens <= _maxTokenBudget) return _history;

    // 2. Score messages
    var scoredHistory = _history
        .Select(msg => new { Message = msg, Score = CalculateCosineSimilarity(...) })
        .OrderByDescending(x => x.Score) // Most relevant first
        .ToList();

    // 3. Fill the bucket
    var optimizedContext = new List<ConversationMessage>();
    int accumulatedTokens = 0;
    foreach (var item in scoredHistory)
    {
        if (accumulatedTokens + item.Message.Content.Length / 4 <= _maxTokenBudget)
        {
            optimizedContext.Add(item.Message);
            accumulatedTokens += item.Message.Content.Length / 4;
        }
    }
    // ...
}
