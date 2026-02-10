
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;

public class ApplicationMonitor
{
    // Static members: Shared by all instances of ApplicationMonitor
    public static int InstanceCount = 0;
    
    // New static field for shared status
    public static string StatusMessage = "Application Running";

    // Instance member: Unique to each object
    public int ID { get; private set; }

    // Constructor
    public ApplicationMonitor()
    {
        InstanceCount++;
        ID = InstanceCount;
        
        // Constructor reads the shared static field
        Console.WriteLine($"Instance {ID} created. Status: {StatusMessage}");
    }

    // New static method to modify the shared field
    public static void UpdateStatus(string newMessage)
    {
        StatusMessage = newMessage;
        Console.WriteLine($"Global status updated to: {StatusMessage}");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Creating first two instances...");
        // These instances are created while StatusMessage is "Application Running"
        ApplicationMonitor monitor1 = new ApplicationMonitor();
        ApplicationMonitor monitor2 = new ApplicationMonitor();

        Console.WriteLine("\nUpdating global status...");
        // This static method call changes the shared variable for the whole class
        ApplicationMonitor.UpdateStatus("Maintenance Mode");

        Console.WriteLine("\nCreating third instance...");
        // This instance is created after the status changed, so it sees "Maintenance Mode"
        ApplicationMonitor monitor3 = new ApplicationMonitor();

        Console.WriteLine($"\nTotal instances created: {ApplicationMonitor.InstanceCount}");
    }
}
