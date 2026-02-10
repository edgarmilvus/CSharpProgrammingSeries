
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

// Conceptual usage of SemaphoreSlim for concurrency control
// This is often used in custom delegating handlers alongside rate limiters
using System.Threading;

public class ModelConcurrencyGate
{
    private readonly SemaphoreSlim _semaphore;

    public ModelConcurrencyGate(int maxConcurrentInferences)
    {
        // Initialize with max concurrent operations allowed
        _semaphore = new SemaphoreSlim(maxConcurrentInferences, maxConcurrentInferences);
    }

    public async Task<T> EnterInferenceAsync<T>(Func<Task<T>> inferenceAction)
    {
        await _semaphore.WaitAsync();
        try
        {
            // Execute the AI model call
            return await inferenceAction();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
