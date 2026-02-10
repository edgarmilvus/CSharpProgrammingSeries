
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TokenUsageTracker
{
    private readonly ConcurrentDictionary<string, int> _usageMap = new ConcurrentDictionary<string, int>();

    // Requirement 3: Atomic Add
    public void AddUsage(string userId, int tokens)
    {
        // AddOrUpdate handles the concurrency internally.
        // If the key doesn't exist, it adds 'tokens'. If it does, it updates by adding 'tokens'.
        _usageMap.AddOrUpdate(userId, tokens, (key, existing) => existing + tokens);
    }

    // Requirement 3: Retrieve total
    public int GetTotalUsage(string userId)
    {
        // TryGetValue is thread-safe for reads
        _usageMap.TryGetValue(userId, out var total);
        return total;
    }

    // Requirement 3: Reset usage
    public void ResetUsage(string userId)
    {
        // TryUpdate ensures we only reset if the value hasn't changed unexpectedly 
        // (though for a reset, usually a simple AddOrUpdate or indexer is sufficient).
        // Using indexer for simplicity here as it's atomic.
        _usageMap[userId] = 0;
    }

    // Requirement 4: Get users near limit
    public List<string> GetUsersNearLimit(int threshold)
    {
        // LINQ on ConcurrentDictionary is safe for enumeration, but represents a snapshot.
        // We iterate over KeyValues to avoid race conditions between reading keys and values.
        return _usageMap
            .Where(kvp => kvp.Value >= threshold)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    // Requirement 6: GetOrAdd pattern
    public int GetOrAddUser(string userId)
    {
        // The valueFactory is only executed if the key is not present.
        // It is safe for the factory to be simple (like returning 0).
        return _usageMap.GetOrAdd(userId, 0);
    }

    // Test Harness
    public static async Task RunTest()
    {
        var tracker = new TokenUsageTracker();
        var random = new Random();
        int totalTokensAdded = 0;
        var tasks = new List<Task>();

        // Simulate 100 threads updating random users
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++) // Each thread does 100 updates
                {
                    int tokens = random.Next(10, 50);
                    Interlocked.Add(ref totalTokensAdded, tokens);
                    string userId = $"User_{random.Next(1, 20)}"; // 20 unique users
                    tracker.AddUsage(userId, tokens);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Verification
        int calculatedTotal = 0;
        for (int i = 1; i < 20; i++)
        {
            calculatedTotal += tracker.GetTotalUsage($"User_{i}");
        }

        Console.WriteLine($"Expected Total Tokens: {totalTokensAdded}");
        Console.WriteLine($"Actual Tracked Tokens: {calculatedTotal}");
        Console.WriteLine($"Match: {totalTokensAdded == calculatedTotal}");

        // Check near limit
        tracker.AddUsage("User_1", 5000); // Force over threshold
        var nearLimit = tracker.GetUsersNearLimit(4000);
        Console.WriteLine($"Users near limit: {string.Join(", ", nearLimit)}");
    }
}
