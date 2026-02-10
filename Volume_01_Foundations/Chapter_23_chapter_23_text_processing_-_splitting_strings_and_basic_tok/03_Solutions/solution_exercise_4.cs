
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

class Program
{
    static void Main()
    {
        string logData = "[ERROR][2023-10-27] Disk space low on drive C:;[WARNING][2023-10-27] Network latency high";

        // 1. Split by semicolon to get individual log entries
        string[] entries = logData.Split(';');

        foreach (string entry in entries)
        {
            // Entry example: [ERROR][2023-10-27] Disk space low on drive C:
            
            // 2. Isolate Level and Date.
            // Split by ']' to separate the bracketed parts from the message.
            // Result: ["[ERROR", "[2023-10-27", " Disk space low..."]
            string[] parts = entry.Split(']');

            // The first part contains the level: "[ERROR"
            // We need to remove the '[' to get just "ERROR"
            string levelPart = parts[0]; 
            string level = levelPart.Substring(1); // Removes the first character '['

            // The third part (index 2) contains the message text
            // Note: Index 1 is the date part, which we might not need for this specific logic
            string fullMessage = parts[2];

            // 3. Parse the Message details
            // We split the message by ':' to get the detail part
            string[] messageParts = fullMessage.Split(':');
            
            // We assume the format is "Summary : Detail". 
            // If there is a colon, we take the part after it. 
            // If not, we just take the whole message.
            string messageDetail = (messageParts.Length > 1) ? messageParts[1] : fullMessage;

            // 4. Filter and Print based on Level
            if (level == "ERROR")
            {
                Console.WriteLine($"[CRITICAL ALERT]: {messageDetail}");
            }
            else if (level == "WARNING")
            {
                Console.WriteLine($"[CAUTION]: {messageDetail}");
            }
        }
    }
}
