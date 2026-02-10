
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

// 1. Define custom exceptions
public class RateLimitExceededException : Exception 
{
    public RateLimitExceededException(string msg) : base(msg) { }
}

public class AuthenticationException : Exception 
{
    public AuthenticationException(string msg) : base(msg) { }
}

public class NetworkTimeoutException : Exception 
{
    public NetworkTimeoutException(string msg) : base(msg) { }
}

public class AIBatchExecutor
{
    // 2. Method that randomly throws exceptions
    public async Task<string> ExecuteTaskAsync()
    {
        await Task.Delay(20);
        var r = new Random(Guid.NewGuid().GetHashCode());
        int val = r.Next(0, 3);

        return val switch
        {
            0 => throw new RateLimitExceededException("Rate limit hit."),
            1 => throw new AuthenticationException("Token expired."),
            2 => throw new NetworkTimeoutException("Network lag."),
            _ => "Success"
        };
    }
}

public class Program
{
    public static async Task Main()
    {
        var executor = new AIBatchExecutor();
        
        // 2. Run 5 parallel tasks
        var tasks = Enumerable.Range(0, 5).Select(_ => executor.ExecuteTaskAsync()).ToList();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (AggregateException ae)
        {
            // 4. Query InnerExceptions using LINQ
            var innerExceptions = ae.Flatten().InnerExceptions;

            // 5. Check for fatal AuthenticationException
            var authEx = innerExceptions.OfType<AuthenticationException>().FirstOrDefault();
            if (authEx != null)
            {
                Console.WriteLine("CRITICAL: Authentication failed. Failing fast.");
                // 7. Use ExceptionDispatchInfo to capture stack trace before re-throwing
                ExceptionDispatchInfo.Capture(authEx).Throw();
            }

            // 6. Handle recoverable exceptions
            var recoverableExs = innerExceptions
                .Where(ex => ex is RateLimitExceededException || ex is NetworkTimeoutException)
                .ToList();

            if (recoverableExs.Any())
            {
                Console.WriteLine($"WARNING: Recoverable errors found ({recoverableExs.Count}).");
                foreach (var ex in recoverableExs)
                {
                    Console.WriteLine($" - Logged: {ex.Message}");
                }
                
                // Return fallback result
                Console.WriteLine("Returning fallback results due to recoverable errors.");
                // In a real scenario, we might return a list of partial results here
            }
        }
    }
}
