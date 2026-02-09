
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

public class LlmClient
{
    // This method simulates calling an external LLM API.
    // It takes a 'cancellationToken' which allows the caller to cancel this operation.
    public async Task<string> GetLlmResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Sending prompt to LLM: '{prompt}'");

        try
        {
            // SIMULATION: We simulate a network request that takes a variable amount of time.
            // In a real scenario, you would pass the 'cancellationToken' to the actual HTTP client call.
            // e.g., await _httpClient.GetAsync(url, cancellationToken);
            // Here, we use Task.Delay to represent the work being done.
            // The 'cancellationToken' will cancel this delay if it triggers.
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] LLM Response Received.");
            return "This is a simulated response from the LLM.";
        }
        catch (OperationCanceledException)
        {
            // This specific exception is thrown when the CancellationToken is canceled.
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] LLM Request was CANCELLED due to timeout.");
            throw; // Re-throw to signal the timeout to the calling method.
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] An unexpected error occurred: {ex.Message}");
            throw;
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- Basic Timeout Example ---");
        var llmClient = new LlmClient();

        // 1. DEFINE THE TIMEOUT
        // We decide that if the LLM takes longer than 3 seconds, we should give up.
        // This is our "patience" threshold.
        var timeoutDuration = TimeSpan.FromSeconds(3);

        // 2. CREATE THE CANCELLATION TOKEN SOURCE
        // This class is the controller. It manages the CancellationToken and can trigger its cancellation.
        // We configure it to automatically cancel after the specified timeout duration.
        using var cts = new CancellationTokenSource(timeoutDuration);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Application started. Timeout is set to {timeoutDuration.TotalSeconds} seconds.");
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] The LLM will take 10 seconds to respond, which is longer than our timeout.");

        try
        {
            // 3. PASS THE TOKEN TO THE ASYNC METHOD
            // We call our LLM client and pass the Token from our source.
            // If the timeout expires, the token will be canceled, and the GetLlmResponseAsync method will be interrupted.
            string response = await llmClient.GetLlmResponseAsync("What is async/await?", cts.Token);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SUCCESS: Received response: {response}");
        }
        catch (OperationCanceledException)
        {
            // 4. HANDLE THE TIMEOUT GRACEFULLY
            // The catch block executes when the timeout is exceeded.
            // Instead of crashing, we can now implement our fallback logic.
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] MAIN: The operation timed out. We will now use a cached response or inform the user.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] MAIN: An unexpected error occurred: {ex.Message}");
        }

        Console.WriteLine("\n--- Example with a Fast Response ---");
        // Let's see what happens when the LLM is fast enough.
        // We create a new token source with the same timeout.
        using var fastCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        try
        {
            // This time, we simulate a fast response by not passing the token to a delay.
            // We'll just create a task that completes quickly.
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Calling a fast LLM...");
            await Task.Delay(TimeSpan.FromSeconds(1), fastCts.Token); // Simulate 1 second work
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SUCCESS: Fast LLM responded in 1 second.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] MAIN: This should not be printed because the task finished in time.");
        }
    }
}
