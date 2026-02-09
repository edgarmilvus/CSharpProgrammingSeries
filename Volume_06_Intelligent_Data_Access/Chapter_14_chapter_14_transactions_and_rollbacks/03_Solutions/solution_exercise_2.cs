
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
using System.Collections.Generic;
using System.Threading.Tasks;

// Context classes for the Saga
public class DocumentContext
{
    public Guid DocumentId { get; set; }
    public string Text { get; set; }
    public float[] Embedding { get; set; }
    public bool TextStored { get; set; }
    public bool VectorStored { get; set; }
}

public class DocumentIndexingSaga
{
    private readonly Random _failureSimulator;

    public DocumentIndexingSaga()
    {
        _failureSimulator = new Random();
    }

    public async Task ExecuteAsync(string content)
    {
        var context = new DocumentContext
        {
            DocumentId = Guid.NewGuid(),
            Text = content
        };

        // Stack to track successful steps for LIFO compensation
        var executedSteps = new Stack<Func<Task>>();

        try
        {
            // Step 1: Store Text (SQL)
            await StoreTextAsync(context);
            executedSteps.Push(() => CompensateTextAsync(context));
            Console.WriteLine("Step 1: Text stored successfully.");

            // Step 2: Generate Embedding (AI Service)
            await GenerateEmbeddingAsync(context);
            // No compensation needed for generation (stateless), but we track it for logic flow
            executedSteps.Push(() => CompensateEmbeddingAsync(context)); 
            Console.WriteLine("Step 2: Embedding generated.");

            // Step 3: Store Vector (Vector Store) - High failure probability
            await StoreVectorAsync(context);
            executedSteps.Push(() => CompensateVectorAsync(context));
            Console.WriteLine("Step 3: Vector stored successfully.");

            Console.WriteLine("Saga Completed Successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Saga failed: {ex.Message}. Starting Compensation...");

            // Execute compensating actions in LIFO order
            while (executedSteps.Count > 0)
            {
                var compensateAction = executedSteps.Pop();
                try
                {
                    await compensateAction();
                }
                catch (Exception compEx)
                {
                    // Log compensation failure (Critical for debugging)
                    Console.WriteLine($"Compensation failed: {compEx.Message}");
                    // In a real system, we might alert an operator or queue for manual intervention
                }
            }
        }
    }

    // --- Forward Operations ---

    private async Task StoreTextAsync(DocumentContext ctx)
    {
        // Simulate DB call
        await Task.Delay(100);
        ctx.TextStored = true;
    }

    private async Task GenerateEmbeddingAsync(DocumentContext ctx)
    {
        // Simulate AI call
        await Task.Delay(100);
        ctx.Embedding = new float[] { 0.1f, 0.2f, 0.3f };
    }

    private async Task StoreVectorAsync(DocumentContext ctx)
    {
        // Simulate Vector Store call with random failure
        await Task.Delay(100);
        if (_failureSimulator.Next(0, 2) == 0) // 50% chance of failure
        {
            throw new InvalidOperationException("Network error: Vector store unreachable.");
        }
        ctx.VectorStored = true;
    }

    // --- Compensating Actions ---

    private async Task CompensateTextAsync(DocumentContext ctx)
    {
        if (ctx.TextStored)
        {
            Console.WriteLine("Compensating: Deleting text from SQL DB...");
            await Task.Delay(50); // Simulate DB delete
            ctx.TextStored = false;
        }
    }

    private async Task CompensateEmbeddingAsync(DocumentContext ctx)
    {
        // Usually nothing to do here as generation is stateless
        Console.WriteLine("Compensating: Clearing generated embedding from memory...");
        ctx.Embedding = null;
    }

    private async Task CompensateVectorAsync(DocumentContext ctx)
    {
        if (ctx.VectorStored)
        {
            Console.WriteLine("Compensating: Deleting vector from Vector Store...");
            await Task.Delay(50); // Simulate Vector Store delete
            ctx.VectorStored = false;
        }
    }
}

// --- Idempotency Discussion (Embedded in Logic) ---

/*
 * IDEMPOTENCY STRATEGY:
 * 
 * 1. The compensating actions (e.g., DeleteTextAsync) must be idempotent. 
 *    If the system crashes during compensation, the recovery process might re-execute the compensation.
 * 
 * 2. Implementation:
 *    - Use a unique Transaction ID (Saga ID) for the entire operation.
 *    - Store the state of the saga (e.g., in a 'SagaLog' table) before executing steps.
 *    - When executing a compensating action, check if the target resource has already been cleaned up.
 *      Example: In 'CompensateTextAsync', check if the record exists before attempting to delete. 
 *      If it's already gone, return successfully (200 OK) rather than throwing a 'NotFound' error.
 * 
 * 3. Retry Policies:
 *    - Exponential backoff should be applied to compensating actions. 
 *    - If a compensation fails (e.g., DB is down), the system should retry until success.
 */
