
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

// This script demonstrates text manipulation to create personalized event invitations.
// We will handle three attendees manually since loops and arrays are not yet available.
// The focus is on using String Interpolation ($), Concatenation (+), and Escape Chars (\n).

public class EventInvitations
{
    public static void Main()
    {
        // ---------------------------------------------------------
        // DATA PREPARATION
        // ---------------------------------------------------------
        // We define variables for the event details.
        // These are strings and integers that represent the real-world event data.
        
        string eventName = "Modern C# Coding Workshop";
        string eventDate = "October 15th, 2024";
        string eventTime = "10:00 AM - 2:00 PM";
        string location = "Community Center, Room 3B";
        
        // Individual Attendee Data (Simulating a database row by row)
        // Attendee 1
        string guest1Name = "Alice";
        string guest1Email = "alice@example.com";
        int guest1TicketId = 1001;
        
        // Attendee 2
        string guest2Name = "Bob";
        string guest2Email = "bob@example.com";
        int guest2TicketId = 1002;
        
        // Attendee 3
        string guest3Name = "Charlie";
        string guest3Email = "charlie@example.com";
        int guest3TicketId = 1003;

        // ---------------------------------------------------------
        // INVITATION GENERATION
        // ---------------------------------------------------------
        // We will now construct the invitations.
        // We use two main techniques:
        // 1. String Interpolation ($): To embed variables directly into the text.
        // 2. String Concatenation (+): To combine separate string literals and variables.
        // 3. Escape Characters (\n): To insert new lines for readability.

        // --- ATTENDEE 1 INVITATION ---
        // Using String Interpolation for a clean, readable format.
        // We use the @ symbol for the multi-line string literal.
        
        string invitation1 = $@"Dear {guest1Name},

You are cordially invited to attend the {eventName}.
Date: {eventDate}
Time: {eventTime}
Location: {location}

Your Ticket ID is: {guest1TicketId}
Please bring this email to the check-in desk.

We look forward to seeing you!
- The Organizing Team";

        // --- ATTENDEE 2 INVITATION ---
        // Using a mix of Interpolation and Concatenation.
        // This shows how to build a string in parts.
        
        string header2 = "Invitation for " + guest2Name + "\n";
        string details2 = "Event: " + eventName + " | " + eventDate + "\n";
        string footer2 = "Location: " + location;
        
        // Combining the parts using the + operator
        string invitation2 = header2 + details2 + footer2;

        // --- ATTENDEE 3 INVITATION ---
        // Using standard Concatenation (without interpolation) for practice.
        // Note: This becomes harder to read with many variables.
        
        string invitation3 = "Dear " + guest3Name + ",\n" +
                             "Welcome to " + eventName + ".\n" +
                             "Date: " + eventDate + "\n" +
                             "Time: " + eventTime + "\n" +
                             "Ticket ID: " + guest3TicketId + "\n" +
                             "Location: " + location + "\n\n" +
                             "Please arrive 15 minutes early.";

        // ---------------------------------------------------------
        // OUTPUT DISPLAY
        // ---------------------------------------------------------
        // We print the invitations to the console.
        // We add separators to distinguish between different guests.

        Console.WriteLine("==========================================");
        Console.WriteLine("GENERATING INVITATIONS...");
        Console.WriteLine("==========================================\n");

        // Display Invitation 1
        Console.WriteLine(invitation1);
        Console.WriteLine("\n------------------------------------------\n");

        // Display Invitation 2
        Console.WriteLine(invitation2);
        Console.WriteLine("\n------------------------------------------\n");

        // Display Invitation 3
        Console.WriteLine(invitation3);
        Console.WriteLine("\n==========================================");
    }
}
