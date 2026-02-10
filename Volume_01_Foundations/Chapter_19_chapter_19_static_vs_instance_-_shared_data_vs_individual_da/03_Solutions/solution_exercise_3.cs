
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;

public class Player
{
    // Static field: Shared by all Player instances. One copy for the whole class.
    public static string GameMode = "Normal";

    // Instance field: Each Player object gets its own copy.
    private int _score;

    // Instance property to access the private score
    public int Score
    {
        get { return _score; }
    }

    // Constructor: Initializes the instance field for this specific player.
    public Player(int startScore)
    {
        _score = startScore;
    }

    // Instance method: Uses both instance data (_score) and static data (GameMode).
    public void UpdateScore(int points)
    {
        // Check the shared static field to decide logic
        if (GameMode == "Hard")
        {
            points = points * 2; // Double points in Hard mode
        }
        
        // Update the specific instance's score
        _score += points;
        Console.WriteLine($"Player score updated to: {_score} (Mode: {GameMode})");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // Create two players with different starting scores
        Player player1 = new Player(0);
        Player player2 = new Player(10);

        Console.WriteLine("--- Playing in Normal Mode ---");
        // In Normal mode, 5 points are added as is
        player1.UpdateScore(5); 
        player2.UpdateScore(5);

        // Change the static field. This affects ALL players, existing and future.
        Player.GameMode = "Hard";
        Console.WriteLine("\n--- Switched to Hard Mode ---");
        
        // In Hard mode, 5 points are doubled to 10 before adding
        player1.UpdateScore(5); 
        player2.UpdateScore(5);

        Console.WriteLine($"\nFinal Scores:");
        Console.WriteLine($"Player 1: {player1.Score}");
        Console.WriteLine($"Player 2: {player2.Score}");
    }
}
