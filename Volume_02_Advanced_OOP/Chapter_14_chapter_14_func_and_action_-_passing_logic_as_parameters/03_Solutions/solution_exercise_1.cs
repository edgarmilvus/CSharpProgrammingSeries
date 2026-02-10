
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
using System.Linq;

public class Enemy
{
    public int Id { get; set; }
    public double Distance { get; set; }
    public int Health { get; set; }
    public int Reward { get; set; }

    // Helper to display enemy details
    public override string ToString()
    {
        return $"Enemy #{Id} (Dist: {Distance}, HP: {Health}, Reward: {Reward})";
    }
}

public class Tower
{
    public Enemy SelectTarget(List<Enemy> enemies, Func<Enemy, double> scoringLogic)
    {
        // Edge Case Handling: Return null if list is null or empty
        if (enemies == null || !enemies.Any())
        {
            return null;
        }

        // Use LINQ to apply the scoring logic and find the enemy with the highest score
        // OrderByDescending sorts the collection based on the key selector (scoringLogic)
        // First() retrieves the top element
        return enemies.OrderByDescending(scoringLogic).First();
    }
}

public class Program
{
    public static void Main()
    {
        // 1. Setup enemies
        var enemies = new List<Enemy>
        {
            new Enemy { Id = 1, Distance = 10.5, Health = 100, Reward = 50 },
            new Enemy { Id = 2, Distance = 5.0,  Health = 20,  Reward = 100 },
            new Enemy { Id = 3, Distance = 20.0, Health = 50,  Reward = 200 }
        };

        var tower = new Tower();

        // 2. Define strategies using Lambda Expressions
        
        // Strategy A: Prioritize Distance (Closer is better)
        // Note: We use 1.0 / Distance to invert the scale (higher score = closer)
        Func<Enemy, double> prioritizeDistance = enemy => 1.0 / enemy.Distance;

        // Strategy B: Prioritize Health (Weaker enemies are better)
        Func<Enemy, double> prioritizeHealth = enemy => enemy.Health;

        // Strategy C: Prioritize Value (Higher reward is better)
        Func<Enemy, double> prioritizeValue = enemy => enemy.Reward;

        // 3. Execute and Print results
        Console.WriteLine("--- Strategy: Distance (Closer is better) ---");
        Enemy targetDistance = tower.SelectTarget(enemies, prioritizeDistance);
        Console.WriteLine(targetDistance != null ? targetDistance.ToString() : "No target found.");

        Console.WriteLine("\n--- Strategy: Health (Weaker is better) ---");
        Enemy targetHealth = tower.SelectTarget(enemies, prioritizeHealth);
        Console.WriteLine(targetHealth != null ? targetHealth.ToString() : "No target found.");

        Console.WriteLine("\n--- Strategy: Value (High Reward is better) ---");
        Enemy targetValue = tower.SelectTarget(enemies, prioritizeValue);
        Console.WriteLine(targetValue != null ? targetValue.ToString() : "No target found.");
    }
}
