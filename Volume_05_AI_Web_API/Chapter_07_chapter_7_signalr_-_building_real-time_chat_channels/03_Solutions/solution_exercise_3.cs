
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

// File: StreamingHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

public class StreamingHub : Hub
{
    /// <summary>
    /// Public entry point for the client to request a streamed response.
    /// </summary>
    public async IAsyncEnumerable<string> GetStreamingResponse(
        string prompt, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Delegate token generation to the helper method
        await foreach (var token in SimulateAiTokenGeneration(prompt, cancellationToken))
        {
            yield return token;
        }
    }

    /// <summary>
    /// Simulates an AI model generating tokens with artificial latency.
    /// </summary>
    private async IAsyncEnumerable<string> SimulateAiTokenGeneration(
        string prompt, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Split prompt into words to simulate tokens
        var tokens = prompt.Split(' ');
        
        foreach (var token in tokens)
        {
            // 1. Check for cancellation before yielding
            cancellationToken.ThrowIfCancellationRequested();

            // 2. Simulate processing time (e.g., LLM inference latency)
            await Task.Delay(150, cancellationToken);

            // 3. Yield the token back to the caller (the SignalR framework)
            yield return token + " "; 
        }

        // Yield a final completion message
        yield return "[Stream Complete]";
    }
}

/*
 * Client-Side Consumption Description:
 * 
 * 1. **Establish Connection:** Initialize the SignalR connection as usual.
 *    const connection = new signalR.HubConnectionBuilder()
 *        .withUrl("/streamingHub")
 *        .build();
 * 
 * 2. **Invoke Stream:** Use the `stream` method on the connection object. 
 *    This returns a `StreamInvocationStream` that implements `IAsyncEnumerable`.
 *    const prompt = "Hello AI world";
 *    const stream = connection.stream("GetStreamingResponse", prompt);
 * 
 * 3. **Consume Tokens:** Use a `for await...of` loop (in JavaScript) to process incoming data.
 *    try {
 *        for await (const token of stream) {
 *            // Append the token to the chat window immediately
 *            document.getElementById('ai-response').innerText += token;
 *        }
 *        console.log("Stream finished.");
 *    } catch (err) {
 *        console.error("Stream error:", err);
 *    }
 * 
 * 4. **Cancellation Logic:**
 *    - **Client Side:** To cancel, the client typically aborts the connection or uses an `AbortController` if using the newer SignalR JS client API that supports it. 
 *    - **Server Side:** The `[EnumeratorCancellation]` attribute on the `CancellationToken` parameter is crucial. It tells the C# compiler to link the cancellation token to the `IAsyncEnumerable` disposal. If the client disconnects or cancels the stream, the token triggers, causing `Task.Delay` to throw an `OperationCanceledException` and breaking the loop.
 */
