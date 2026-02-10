
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
using System.Threading;
using System.Threading.Tasks;

// Simulates a legacy, non-thread-safe library
public class LegacyTokenizer
{
    // Simulate a heavy CPU-bound task
    public string Tokenize(string text)
    {
        Console.WriteLine($"Starting tokenization of {text.Length} chars on Thread {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(2000); // Simulate CPU work
        return $"TOKENIZED_{text}";
    }
}

public class TokenizationService
{
    private readonly LegacyTokenizer _tokenizer;
    // SemaphoreSlim with a count of 1 ensures only one thread accesses the tokenizer at a time
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public TokenizationService(LegacyTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }

    public async Task<string> TokenizeAsync(string text, CancellationToken cancellationToken)
    {
        // Acquire the semaphore asynchronously
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Offload the CPU-bound work to a thread pool thread to avoid blocking the event loop.
            // We wrap the synchronous call in Task.Run.
            return await Task.Run(() => _tokenizer.Tokenize(text), cancellationToken);
        }
        finally
        {
            // Ensure the semaphore is released even if an exception occurs
            _semaphore.Release();
        }
    }
}
