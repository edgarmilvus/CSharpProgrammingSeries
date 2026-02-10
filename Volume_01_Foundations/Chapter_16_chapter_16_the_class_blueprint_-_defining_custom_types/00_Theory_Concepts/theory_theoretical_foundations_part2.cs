
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

// Source File: theory_theoretical_foundations_part2.cs
// Description: Theoretical Foundations
// ==========================================

using System;

class Robot
{
    // 1. Fields (Data)
    // These are variables that belong to the class. They store the state.
    // We use access modifiers like 'private' to protect data (Encapsulation).
    private string model;
    private int batteryLevel;

    // 2. Constructor (Initialization)
    // This special method runs when we create a new Robot.
    // It sets up the initial state.
    public Robot(string initialModel, int initialBattery)
    {
        model = initialModel;
        batteryLevel = initialBattery;
    }

    // 3. Methods (Behavior)
    // These define what the Robot can do.
    public void DisplayStatus()
    {
        // We access the fields using 'this' to refer to the current object
        Console.WriteLine($"Model: {model}, Battery: {batteryLevel}%");
    }

    public void ConsumeEnergy(int amount)
    {
        // We can modify the fields
        batteryLevel = batteryLevel - amount;
        
        // Logic from Chapter 6: if statements
        if (batteryLevel < 0)
        {
            batteryLevel = 0;
        }
    }
}
