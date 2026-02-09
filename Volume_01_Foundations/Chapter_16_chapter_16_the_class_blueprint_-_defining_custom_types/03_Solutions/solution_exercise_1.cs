
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

// 1. Define the class blueprint
public class BankAccount
{
    // Fields to store the state of the object
    public string accountHolderName;
    public double balance;
}

public class Program
{
    public static void Main()
    {
        // 2. Create two instances of the BankAccount class using 'new'
        BankAccount aliceAccount = new BankAccount();
        BankAccount bobAccount = new BankAccount();

        // 3. Assign values to the fields of the first instance
        aliceAccount.accountHolderName = "Alice";
        aliceAccount.balance = 1500.50;

        // 3. Assign values to the fields of the second instance
        bobAccount.accountHolderName = "Bob";
        bobAccount.balance = 250.00;

        // 4. Print the details using String Interpolation
        Console.WriteLine($"Account 1: {aliceAccount.accountHolderName} has ${aliceAccount.balance}");
        Console.WriteLine($"Account 2: {bobAccount.accountHolderName} has ${bobAccount.balance}");
    }
}
