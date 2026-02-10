
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

public class BankAccount
{
    // Properties (Read-only)
    public string AccountNumber { get; }
    public string AccountHolder { get; }
    public double Balance { get; private set; } // Private set allows updates via methods
    public bool IsActive { get; private set; }

    // Constructor
    public BankAccount(string number, string holder, double initialBalance)
    {
        // Validation: Check if initial balance is negative
        if (initialBalance < 0)
        {
            throw new Exception("Initial balance cannot be negative.");
        }

        // Assignment: Initialize read-only properties
        AccountNumber = number;
        AccountHolder = holder;
        Balance = initialBalance;
        IsActive = true; // Default initialization
    }

    // Methods
    public void Deposit(double amount)
    {
        if (amount > 0)
        {
            Balance += amount;
            Console.WriteLine($"Deposited: ${amount}");
        }
    }

    public void Withdraw(double amount)
    {
        if (amount > 0)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                Console.WriteLine($"Withdrew: ${amount}");
            }
            else
            {
                Console.WriteLine("Insufficient funds.");
            }
        }
    }

    public void DisplayStatus()
    {
        string status = IsActive ? "Active" : "Inactive";
        // Using format specifier F2 to ensure 2 decimal places for currency
        Console.WriteLine($"Account: {AccountNumber} | Holder: {AccountHolder} | Balance: ${Balance:F2} | Status: {status}");
    }
}

public class Program
{
    public static void Main()
    {
        // Test Case 1: Valid Account
        try
        {
            BankAccount acc1 = new BankAccount("12345", "Alice", 500.00);
            acc1.DisplayStatus();
            acc1.Deposit(150.00);
            acc1.Withdraw(200.00);
            acc1.DisplayStatus();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Test Case 2: Invalid Initial Balance
        try
        {
            BankAccount acc2 = new BankAccount("67890", "Bob", -100.00);
            acc2.DisplayStatus();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating account for Bob: {ex.Message}");
        }
    }
}
