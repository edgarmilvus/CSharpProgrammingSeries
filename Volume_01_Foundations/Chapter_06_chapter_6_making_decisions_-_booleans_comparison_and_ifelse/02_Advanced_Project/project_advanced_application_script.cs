
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;

// This script simulates a simple "Smart Home Security System" access control panel.
// It demonstrates complex decision-making using only the concepts from Chapter 6.
// The system will check a user's ID, PIN, and time of day to grant or deny access.

class SecuritySystem
{
    static void Main()
    {
        // ==========================================================
        // SCENARIO SETUP: Defining the authorized users and secrets
        // ==========================================================

        // User 1: The Home Owner
        int ownerID = 1001;
        int ownerPIN = 5555;
        bool ownerIsAdmin = true;

        // User 2: The House Sitter
        int sitterID = 2002;
        int sitterPIN = 1234;
        bool sitterIsAdmin = false;

        // User 3: The Neighbor (Emergency Access)
        int neighborID = 3003;
        int neighborPIN = 9999;
        bool neighborIsAdmin = false;

        // System State Variables
        int systemTime = 14; // 24-hour format (e.g., 14 = 2:00 PM)
        string accessResult = "Denied"; // Default state
        string logMessage = "System Initialized. Waiting for input.";

        // ==========================================================
        // INPUT PHASE: Getting credentials from the user
        // ==========================================================

        Console.WriteLine("--- SMART HOME SECURITY PANEL ---");
        Console.WriteLine("Please enter your User ID:");
        
        // We read the string input and convert it to an integer immediately.
        // We use 'Convert' here as per Chapter 5 capabilities.
        string inputIDString = Console.ReadLine();
        int inputID = Convert.ToInt32(inputIDString);

        Console.WriteLine("Please enter your 4-digit PIN:");
        
        string inputPINString = Console.ReadLine();
        int inputPIN = Convert.ToInt32(inputPINString);

        // ==========================================================
        // LOGIC PHASE: The Decision Tree
        // ==========================================================

        // BLOCK 1: Verify Identity
        // We check if the ID matches ANY of the known users.
        // Since we cannot use arrays or loops, we must check each one individually.
        
        // Check User 1 (Owner)
        if (inputID == ownerID)
        {
            // Identity confirmed, now verify PIN
            if (inputPIN == ownerPIN)
            {
                // PIN is correct.
                accessResult = "GRANTED";
                logMessage = "Welcome Home, Admin.";
            }
            else
            {
                // ID matched, but PIN was wrong.
                accessResult = "DENIED";
                logMessage = "Invalid PIN for Owner ID.";
            }
        }
        // Check User 2 (Sitter) - Note the 'else if' structure
        else if (inputID == sitterID)
        {
            if (inputPIN == sitterPIN)
            {
                // PIN is correct. Now check time restrictions for non-admins.
                // Sitter is allowed between 8 AM and 6 PM (08 to 18).
                
                if (systemTime >= 8 && systemTime < 18)
                {
                    accessResult = "GRANTED";
                    logMessage = "Welcome, House Sitter. Stick to daylight hours.";
                }
                else
                {
                    // Time is outside allowed range
                    accessResult = "DENIED";
                    logMessage = "Access Restricted: Sitter allowed 8AM-6PM only.";
                }
            }
            else
            {
                accessResult = "DENIED";
                logMessage = "Invalid PIN for Sitter ID.";
            }
        }
        // Check User 3 (Neighbor)
        else if (inputID == neighborID)
        {
            if (inputPIN == neighborPIN)
            {
                // Neighbor has emergency access, but let's check if it's too late at night.
                // Neighbor allowed only between 9 AM and 9 PM (09 to 21).
                
                if (systemTime >= 9 && systemTime < 21)
                {
                    accessResult = "GRANTED";
                    logMessage = "Emergency Access Opened. Please check the back door.";
                }
                else
                {
                    accessResult = "DENIED";
                    logMessage = "Emergency Access Locked: Too late for Neighbor.";
                }
            }
            else
            {
                accessResult = "DENIED";
                logMessage = "Invalid PIN for Neighbor ID.";
            }
        }
        // BLOCK 2: Unknown User
        // If none of the 'if' or 'else if' conditions above were met, we land here.
        else
        {
            accessResult = "DENIED";
            logMessage = "Unknown ID detected. Security Alert Triggered.";
        }

        // ==========================================================
        // OUTPUT PHASE: Displaying the result
        // ==========================================================

        Console.WriteLine(""); // Empty line for spacing
        Console.WriteLine("--- VERIFICATION RESULT ---");
        Console.WriteLine($"Status: {accessResult}");
        Console.WriteLine($"System Log: {logMessage}");
        
        // Example of a secondary check based on the result
        if (accessResult == "GRANTED")
        {
            Console.WriteLine("Disarming alarm system...");
            Console.WriteLine("Unlocking main door...");
        }
        else
        {
            Console.WriteLine("Maintaining lockdown mode.");
        }
    }
}
