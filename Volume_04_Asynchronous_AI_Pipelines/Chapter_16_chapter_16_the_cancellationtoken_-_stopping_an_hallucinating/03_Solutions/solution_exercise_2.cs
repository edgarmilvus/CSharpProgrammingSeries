
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

public class LinkedTokenExercise
{
    public async Task<string> PreprocessInputAsync(string input, CancellationToken token)
    {
        // Simulate work with Task.Delay(100, token)
        await Task.Delay(100, token);
        token.ThrowIfCancellationRequested();
        return input.ToUpper();
    }

    public async Task<string> InferModelAsync(string processedInput, CancellationToken token)
    {
        // Simulate work with Task.Delay(200, token)
        await Task.Delay(200, token);
        token.ThrowIfCancellationRequested();
        return $"ModelResponse_{processedInput}";
    }

    public async Task<string> PostprocessOutputAsync(string modelOutput, CancellationToken token)
    {
        // Simulate work with Task.Delay(100, token)
        await Task.Delay(100, token);
        token.ThrowIfCancellationRequested();
        return modelOutput + "_FINAL";
    }

    public async Task RunPipeline()
    {
        // 1. Create a timeout token (500ms total budget)
        using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token;
        
        // 2. Create a manual token for user intervention
        using var manualToken = new CancellationTokenSource();
        
        // Simulate manual cancellation after 150ms
        _ = Task.Run(async () => 
        {
            await Task.Delay(150);
            manualToken.Cancel();
        });

        // 3. Create linked token source combining both
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken, manualToken.Token);
        var linkedToken = linkedTokenSource.Token;

        try
        {
            // Pass the linked token to all stages
            var result = await RunStages(linkedToken);
            Console.WriteLine($"Success: {result}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Pipeline canceled.");
        }
    }

    private async Task<string> RunStages(CancellationToken token)
    {
        // The linked token is passed down to every async operation
        var preprocessed = await PreprocessInputAsync("input", token);
        var inferred = await InferModelAsync(preprocessed, token);
        var postprocessed = await PostprocessOutputAsync(inferred, token);
        return postprocessed;
    }
}
