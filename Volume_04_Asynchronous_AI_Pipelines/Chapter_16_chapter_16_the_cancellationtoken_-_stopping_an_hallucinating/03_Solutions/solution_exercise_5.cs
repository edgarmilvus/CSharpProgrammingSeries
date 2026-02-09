
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

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class DualCancellationExercise
{
    public async Task<(string Result, string Status)> GenerateWithDualCancellation(
        CancellationToken timeoutToken, 
        CancellationToken userToken)
    {
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken, userToken);
        var linkedToken = linkedSource.Token;

        var resultBuilder = new StringBuilder();

        try
        {
            for (int i = 0; i < 100; i++)
            {
                // Simulate generation
                await Task.Delay(100, linkedToken);
                resultBuilder.Append($"Token_{i} ");
                
                // Manual check
                linkedToken.ThrowIfCancellationRequested();
            }
            
            return (resultBuilder.ToString(), "Success");
        }
        catch (OperationCanceledException)
        {
            // Prioritize userToken if both are requested
            if (userToken.IsCancellationRequested)
            {
                return (resultBuilder.ToString(), "Stopped");
            }
            else if (timeoutToken.IsCancellationRequested)
            {
                return (resultBuilder.ToString(), "Timeout");
            }
            
            return (resultBuilder.ToString(), "Unknown");
        }
    }

    public async Task RunExercise()
    {
        // Scenario 1: Timeout fires first (250ms delay vs 100ms loop step)
        // The loop runs approx 2.5 times before timeout.
        using var timeout1 = new CancellationTokenSource(250);
        using var user1 = new CancellationTokenSource();
        
        var result1 = await GenerateWithDualCancellation(timeout1.Token, user1.Token);
        Console.WriteLine($"Scenario 1: {result1.Status}, Result: '{result1.Result.Trim()}'");

        // Scenario 2: User cancels first (150ms user vs 2000ms timeout)
        using var timeout2 = new CancellationTokenSource(2000);
        using var user2 = new CancellationTokenSource(150);
        
        var result2 = await GenerateWithDualCancellation(timeout2.Token, user2.Token);
        Console.WriteLine($"Scenario 2: {result2.Status}, Result: '{result2.Result.Trim()}'");
    }
}
