
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;

namespace Chapter16_BasicExample
{
    // 1. The Class Blueprint
    // This defines what a "SimpleBankAccount" looks like.
    // It is not an account itself; it is the template for creating accounts.
    public class SimpleBankAccount
    {
        // 2. Fields (Data Storage)
        // These variables hold the state of the object.
        // We use 'double' to allow for decimal values (e.g., 100.50).
        // We mark them 'private' so they can only be changed through specific methods.
        private double _balance;
        private string _accountHolderName;

        // 3. Constructor
        // This special method runs automatically when we create a new instance.
        // It sets the initial values for the fields.
        public SimpleBankAccount(string name, double startingBalance)
        {
            _accountHolderName = name;
            _balance = startingBalance;
        }

        // 4. Methods (Behavior)
        // These define what the object can do.

        // A method to add money to the account.
        public void Deposit(double amount)
        {
            // We use simple arithmetic to update the field.
            _balance = _balance + amount;
            Console.WriteLine($"Deposited: {amount}. New Balance: {_balance}");
        }

        // A method to check the current balance.
        public void DisplayBalance()
        {
            Console.WriteLine($"Account Holder: {_accountHolderName}");
            Console.WriteLine($"Current Balance: {_balance}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 5. Instantiation (Creating an Object)
            // We use the 'new' keyword to create an actual instance (object) 
            // based on the SimpleBankAccount blueprint.
            // We pass arguments to the constructor to initialize the object.
            SimpleBankAccount myAccount = new SimpleBankAccount("Alice", 1000.00);

            // 6. Using the Object
            // We call methods on the specific object 'myAccount'.
            
            // Check initial balance
            myAccount.DisplayBalance();
            Console.WriteLine(); // Adds a blank line for readability

            // Perform an action (Deposit)
            myAccount.Deposit(500.00);
            Console.WriteLine();

            // Check balance again to see the change
            myAccount.DisplayBalance();
        }
    }
}
