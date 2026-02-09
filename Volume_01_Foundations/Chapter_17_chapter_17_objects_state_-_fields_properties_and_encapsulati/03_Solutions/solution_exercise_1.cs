
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;

// Class definition for BankAccount
public class BankAccount
{
    // Private field: accessible only within this class
    private double _balance;

    // Public property: Read-only access to the private field
    public double Balance
    {
        get { return _balance; }
    }

    // Method to deposit money
    public void Deposit(double amount)
    {
        // Validation: Amount must be positive
        if (amount > 0)
        {
            _balance += amount;
            Console.WriteLine($"Deposited: {amount:C}. New Balance: {_balance:C}");
        }
        else
        {
            Console.WriteLine("Error: Deposit amount must be positive.");
        }
    }

    // Method to withdraw money
    public void Withdraw(double amount)
    {
        // Validation: Amount must be positive AND less than or equal to balance
        // Using logical AND (&&) to check both conditions
        if (amount > 0 && amount <= _balance)
        {
            _balance -= amount;
            Console.WriteLine($"Withdrew: {amount:C}. New Balance: {_balance:C}");
        }
        else
        {
            Console.WriteLine("Error: Invalid withdrawal amount.");
        }
    }
}

public class Program
{
    public static void Main()
    {
        // 1. Create an instance of BankAccount
        BankAccount myAccount = new BankAccount();

        // 2. Deposit 100.50
        myAccount.Deposit(100.50);

        // 3. Print the current balance
        Console.WriteLine($"Current Balance: {myAccount.Balance:C}");

        // 4. Withdraw 50.25
        myAccount.Withdraw(50.25);

        // 5. Print the current balance
        Console.WriteLine($"Current Balance: {myAccount.Balance:C}");

        // 6. Attempt to withdraw 100.00 (should fail due to insufficient funds)
        myAccount.Withdraw(100.00);

        // 7. Attempt to deposit -20.00 (should fail due to invalid amount)
        myAccount.Deposit(-20.00);
    }
}
