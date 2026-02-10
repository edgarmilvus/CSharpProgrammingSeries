
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
using System.Diagnostics;

public struct CriticalEvent
{
    public int Id;
}

public struct NormalEvent
{
    public int Id;
}

public class EventProcessor
{
    // Method 1: 'is' check followed by explicit cast
    public void HandleWithIs(object e)
    {
        if (e is CriticalEvent)
        {
            // Unboxing cast
            CriticalEvent ce = (CriticalEvent)e;
            var dummy = ce.Id; 
        }
    }

    // Method 2: 'as' check (requires nullable value type)
    public void HandleWithAs(object e)
    {
        // 'as' is valid for nullable value types
        CriticalEvent? ce = e as CriticalEvent?; 
        
        if (ce.HasValue)
        {
            var dummy = ce.Value.Id;
        }
    }
}

public class PerformanceTest
{
    public static void Main()
    {
        EventProcessor proc = new EventProcessor();
        object boxedCritical = new CriticalEvent { Id = 1 };
        object boxedNormal = new NormalEvent { Id = 2 };

        int iterations = 1000000;
        Stopwatch sw = new Stopwatch();

        // Test 'is' + cast
        sw.Start();
        for (int i = 0; i < iterations; i++)
        {
            proc.HandleWithIs(boxedCritical);
            proc.HandleWithIs(boxedNormal);
        }
        sw.Stop();
        Console.WriteLine($"Time with 'is' + cast: {sw.ElapsedMilliseconds} ms");

        // Test 'as'
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            proc.HandleWithAs(boxedCritical);
            proc.HandleWithAs(boxedNormal);
        }
        sw.Stop();
        Console.WriteLine($"Time with 'as': {sw.ElapsedMilliseconds} ms");

        // Discussion:
        // For boxed value types, 'is' is often highly optimized.
        // 'as' involves a null check against the returned reference.
        // However, the real cost here is the Boxing itself, not the check.
    }
}
