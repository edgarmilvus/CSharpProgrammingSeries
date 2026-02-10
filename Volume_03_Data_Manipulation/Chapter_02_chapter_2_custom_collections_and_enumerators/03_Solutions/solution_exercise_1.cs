
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
using System.Diagnostics;
using System.Linq; // Used only for result formatting in Main, logic avoids LINQ

public class VocabularyBenchmark
{
    public static void Run()
    {
        int size = 100000;
        List<string> listVocab = new List<string>(size);
        HashSet<string> hashVocab = new HashSet<string>();

        // 1. Populate
        // We use a standard loop to fill both collections with dummy tokens.
        for (int i = 0; i < size; i++)
        {
            string token = $"token_{i}";
            listVocab.Add(token);
            hashVocab.Add(token);
        }

        string targetHit = $"token_{size - 1}"; // Exists at the very end
        string targetMiss = "unknown_token";    // Does not exist

        // --- 2. Benchmark List (Manual Search) ---
        // We manually iterate to simulate the Contains logic.
        var sw = Stopwatch.StartNew();
        bool found = false;
        foreach (var t in listVocab)
        {
            if (t == targetHit) { found = true; break; }
        }
        sw.Stop();
        Console.WriteLine($"List Search (Hit): {sw.ElapsedTicks} ticks. Found: {found}");

        sw.Restart();
        found = false;
        foreach (var t in listVocab)
        {
            if (t == targetMiss) { found = true; break; }
        }
        sw.Stop();
        Console.WriteLine($"List Search (Miss): {sw.ElapsedTicks} ticks. Found: {found}");

        // --- 3. Benchmark HashSet ---
        // We use the built-in Contains which utilizes the hashing algorithm.
        sw.Restart();
        found = hashVocab.Contains(targetHit);
        sw.Stop();
        Console.WriteLine($"HashSet Search (Hit): {sw.ElapsedTicks} ticks. Found: {found}");

        sw.Restart();
        found = hashVocab.Contains(targetMiss);
        sw.Stop();
        Console.WriteLine($"HashSet Search (Miss): {sw.ElapsedTicks} ticks. Found: {found}");
    }
}
