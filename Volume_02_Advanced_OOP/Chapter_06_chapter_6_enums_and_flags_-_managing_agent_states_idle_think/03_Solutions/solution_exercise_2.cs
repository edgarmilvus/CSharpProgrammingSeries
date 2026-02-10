
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

[Flags]
public enum NodeStatus
{
    None = 0,
    Online = 1,
    Processing = 2,
    Syncing = 4,
    LowBattery = 8
}

public class NetworkNode
{
    public NodeStatus Status { get; private set; } = NodeStatus.None;

    // Adds a flag using bitwise OR
    public void AddStatus(NodeStatus status)
    {
        Status |= status;
    }

    // Removes a flag using bitwise AND with the complement
    public void RemoveStatus(NodeStatus status)
    {
        Status &= ~status;
    }

    // Checks if a specific flag (or combination) is set
    public bool HasStatus(NodeStatus status)
    {
        return (Status & status) == status;
    }

    public string GetHealthReport()
    {
        // Priority check: LowBattery overrides other statuses
        if (HasStatus(NodeStatus.LowBattery))
        {
            return "CRITICAL: Low Battery Detected.";
        }
        
        if (HasStatus(NodeStatus.Online))
        {
            return "System Online. Current Operations: " + Status;
        }

        return "System Offline.";
    }
}

// Test Harness
public class Program
{
    public static void Main()
    {
        NetworkNode node = new NetworkNode();

        // 1. Online and Processing
        node.AddStatus(NodeStatus.Online);
        node.AddStatus(NodeStatus.Processing);
        Console.WriteLine($"State 1: {node.Status}"); // Output: Online | Processing (3)

        // 2. Add Syncing
        node.AddStatus(NodeStatus.Syncing);
        Console.WriteLine($"State 2: {node.Status}"); // Output: Online | Processing | Syncing (7)

        // 3. Remove Processing
        node.RemoveStatus(NodeStatus.Processing);
        Console.WriteLine($"State 3: {node.Status}"); // Output: Online | Syncing (5)

        // 4. Add LowBattery
        node.AddStatus(NodeStatus.LowBattery);
        Console.WriteLine($"Health Report: {node.GetHealthReport()}"); // Output: CRITICAL
        
        // Verify composite check
        bool isSyncing = node.HasStatus(NodeStatus.Syncing);
        bool isOnline = node.HasStatus(NodeStatus.Online);
        Console.WriteLine($"Is Syncing: {isSyncing}, Is Online: {isOnline}");
    }
}
