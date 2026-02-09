
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;

// 1. CLASS DEFINITION
// We define a blueprint for bank accounts.
public class BankAccount
{
    // 2. STATIC MEMBER (Shared Data)
    // The 'static' keyword means this variable belongs to the class itself,
    // not to any specific object. It is shared by all instances.
    // We use 'int' for whole numbers.
    public static int TotalAccountsCreated = 0;

    // 3. INSTANCE MEMBERS (Individual Data)
    // These fields belong to a specific object created using 'new'.
    // Each account has its own balance and owner name.
    public double Balance; // 'double' handles decimal numbers
    public string OwnerName;

    // 4. CONSTRUCTOR
    // This runs automatically when we create a new account (using 'new').
    public BankAccount(string name, double startingBalance)
    {
        // Assign the individual data to this specific instance
        this.OwnerName = name;
        this.Balance = startingBalance;

        // Modify the SHARED static data.
        // Every time a new account is created, we increment the global counter.
        // Notice we don't use 'this.' for static members.
        TotalAccountsCreated++;
    }

    // 5. INSTANCE METHOD
    // This action affects only the specific account's balance.
    public void Deposit(double amount)
    {
        // Update the instance field
        this.Balance += amount;
        Console.WriteLine($"{this.OwnerName} deposited ${amount}. New Balance: ${this.Balance}");
    }

    // 6. STATIC METHOD
    // This action relates to the shared data, not a specific account.
    // It doesn't need a specific account to run.
    public static void DisplayBankStats()
    {
        // We can access the static variable directly
        Console.WriteLine($"--- BANK STATS ---");
        Console.WriteLine($"Total Active Accounts: {TotalAccountsCreated}");
        Console.WriteLine("-------------------");
    }
}

public class Program
{
    public static void Main()
    {
        // 7. CREATING INSTANCES
        // We create two distinct objects. They exist separately in memory.
        
        // First Account
        BankAccount aliceAccount = new BankAccount("Alice", 100.0);
        
        // Second Account
        BankAccount bobAccount = new BankAccount("Bob", 50.0);

        // 8. MODIFYING INSTANCE DATA
        // Only Alice's balance changes here. Bob's remains untouched.
        aliceAccount.Deposit(200.0);

        // 9. ACCESSING STATIC DATA
        // We call the static method on the CLASS, not on a specific variable.
        // It shows the total count (2), proving the static variable is shared.
        BankAccount.DisplayBankStats();

        // 10. VERIFYING ISOLATION
        // Let's check Bob's balance to prove it wasn't affected by Alice's deposit.
        Console.WriteLine($"{bobAccount.OwnerName}'s Balance: ${bobAccount.Balance}");
    }
}
