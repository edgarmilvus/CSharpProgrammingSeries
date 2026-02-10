
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class BasicCancellationExercise
{
    public async IAsyncEnumerable<string> GenerateTextAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return "Start";
        
        // Simulate 100 tokens
        for (int i = 0; i < 100; i++)
        {
            // Wait 50ms, checking cancellation via the Task.Delay overload
            // This allows cancellation to interrupt the delay immediately
            await Task.Delay(50, cancellationToken);
            
            // Explicit check after delay (though Delay already throws)
            cancellationToken.ThrowIfCancellationRequested();
            
            yield return $"Token_{i}";
        }
    }

    public async Task RunExercise()
    {
        // Create a token source that cancels after 200ms
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        
        try
        {
            // Iterate through the async stream
            await foreach (var token in GenerateTextAsync(cts.Token))
            {
                Console.WriteLine(token);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Generation was canceled.");
        }
    }
}
