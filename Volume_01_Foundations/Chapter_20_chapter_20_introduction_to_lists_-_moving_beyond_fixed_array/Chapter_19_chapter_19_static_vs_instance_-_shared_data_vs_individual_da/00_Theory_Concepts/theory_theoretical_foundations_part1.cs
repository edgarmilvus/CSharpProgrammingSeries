
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System;

public class BankAccount
{
    // Static field: Shared by all bank accounts.
    // Represents the total money held by the bank.
    public static double TotalBankAssets = 0;

    // Instance field: Unique to each specific account.
    public double Balance;

    public BankAccount(double initialDeposit)
    {
        this.Balance = initialDeposit;
        
        // We add to the static field. 
        // This affects the total for the ENTIRE bank.
        BankAccount.TotalBankAssets += initialDeposit;
    }
}

public class Program
{
    public static void Main()
    {
        // Create first account
        BankAccount aliceAccount = new BankAccount(1000);
        Console.WriteLine($"Alice Balance: {aliceAccount.Balance}");
        Console.WriteLine($"Total Bank Assets: {BankAccount.TotalBankAssets}");

        // Create second account
        BankAccount bobAccount = new BankAccount(500);
        Console.WriteLine($"Bob Balance: {bobAccount.Balance}");
        
        // Notice: TotalAssets increased by 500, even though we didn't 
        // touch the static field directly in this line.
        Console.WriteLine($"Total Bank Assets: {BankAccount.TotalBankAssets}");
    }
}
