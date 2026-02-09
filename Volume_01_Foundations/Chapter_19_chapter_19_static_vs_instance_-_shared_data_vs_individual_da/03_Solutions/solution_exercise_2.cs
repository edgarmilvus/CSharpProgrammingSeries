
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;

public class BankAccount
{
    // Instance field: Each object (instance) gets its own separate copy of this variable in memory.
    private decimal _balance;

    // Instance property: Provides read-only access to the private field.
    public decimal Balance
    {
        get { return _balance; }
    }

    // Constructor: Initializes the instance field for a specific object.
    public BankAccount(decimal initialBalance)
    {
        _balance = initialBalance;
    }

    // Instance method: Operates on the data of the specific object it is called on.
    public void Deposit(decimal amount)
    {
        _balance += amount;
        Console.WriteLine($"Deposited {amount}. New balance: {_balance}");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // Create two distinct instances of BankAccount.
        // 'account1' has its own _balance field in memory.
        // 'account2' has a separate _balance field in memory.
        BankAccount account1 = new BankAccount(100.00m);
        BankAccount account2 = new BankAccount(50.00m);

        // Call Deposit on account1. This modifies only account1's _balance.
        account1.Deposit(25.00m); 
        
        // Call Deposit on account2. This modifies only account2's _balance.
        account2.Deposit(10.00m);

        // Access the Balance property of each specific instance to retrieve their unique values.
        Console.WriteLine($"Account 1 Final Balance: {account1.Balance}");
        Console.WriteLine($"Account 2 Final Balance: {account2.Balance}");
    }
}
