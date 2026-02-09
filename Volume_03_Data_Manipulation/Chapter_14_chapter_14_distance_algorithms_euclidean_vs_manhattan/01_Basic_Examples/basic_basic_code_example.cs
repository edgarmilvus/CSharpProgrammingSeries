
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
using System.Collections.Generic;
using System.Linq;

public static class DistanceCalculator
{
    // 1. Define a simple record to hold user data (Vector representation).
    // Using 'record' for immutability aligns with functional programming principles.
    public record UserVector(string Name, double SciFiRating, double RomanceRating);

    public static void Main()
    {
        // 2. The Dataset: A collection of user vectors.
        // In a real scenario, this might come from a database or CSV file.
        var users = new List<UserVector>
        {
            new UserVector("Alice",  9.0, 2.0), // Target user: loves Sci-Fi, hates Romance
            new UserVector("Bob",    8.5, 2.5), // Very similar to Alice
            new UserVector("Charlie",3.0, 9.0), // Opposite taste
            new UserVector("Diana",  8.0, 3.0)  // Similar to Alice
        };

        // 3. Identify the target user (Alice).
        // We use First() which is an immediate execution to fetch the specific object.
        var targetUser = users.First(u => u.Name == "Alice");

        // 4. Define the Distance Calculation Functions (Pure Functions).
        // These calculate the distance between two vectors (u1 and u2).
        
        // Euclidean Distance: Straight-line distance (Pythagorean theorem).
        // Formula: sqrt(sum((u1_i - u2_i)^2))
        Func<UserVector, UserVector, double> euclideanDistance = (u1, u2) =>
        {
            // We project the differences into a sequence, square them, sum them, and sqrt.
            var differences = new[] 
            { 
                u1.SciFiRating - u2.SciFiRating, 
                u1.RomanceRating - u2.RomanceRating 
            };

            // Immediate Execution: .Sum() processes the collection immediately.
            double sumOfSquares = differences.Select(d => d * d).Sum();
            return Math.Sqrt(sumOfSquares);
        };

        // Manhattan Distance: Sum of absolute differences (Grid-like path).
        // Formula: sum(|u1_i - u2_i|)
        Func<UserVector, UserVector, double> manhattanDistance = (u1, u2) =>
        {
            var differences = new[] 
            { 
                Math.Abs(u1.SciFiRating - u2.SciFiRating), 
                Math.Abs(u1.RomanceRating - u2.RomanceRating) 
            };

            // Immediate Execution: .Sum() processes the collection immediately.
            return differences.Sum();
        };

        // 5. Build the LINQ Pipeline (Deferred Execution).
        // We define the query here, but it does NOT execute yet.
        // This pipeline filters out the target user and calculates distances.
        var similarityQuery = users
            .Where(u => u.Name != targetUser.Name) // Filter: Exclude Alice herself
            .Select(u => new 
            { 
                Name = u.Name, 
                // Calculate both distances for comparison
                Euclidean = euclideanDistance(u, targetUser), 
                Manhattan = manhattanDistance(u, targetUser) 
            });

        // 6. Materialize the results (Immediate Execution).
        // The query executes here because .ToList() forces iteration.
        var results = similarityQuery.ToList();

        // 7. Display Results using a functional pipeline (ForEach is forbidden, so we project to strings).
        // We use string.Join to aggregate the output without side effects in the query.
        var outputLines = results
            .Select(r => $"User: {r.Name,-8} | Euclidean: {r.Euclidean:F2} | Manhattan: {r.Manhattan:F2}")
            .ToList();

        Console.WriteLine($"Comparing to Target: {targetUser.Name} [Sci-Fi: {targetUser.SciFiRating}, Romance: {targetUser.RomanceRating}]");
        Console.WriteLine(string.Join(Environment.NewLine, outputLines));
    }
}
