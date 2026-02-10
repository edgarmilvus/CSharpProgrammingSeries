
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

public class Program
{
    // 'globalBalance' is a variable declared at the class level (Global Scope).
    // It will exist as long as the Program class exists.
    // We will use this to demonstrate how variables behave differently depending on where they are declared.
    public static int globalBalance = 100;

    public static void Main()
    {
        // --- BLOCK 1: The Stack (Local Value Types) ---
        
        // 'itemPrice' is a local variable of type 'int' (Value Type).
        // It is created immediately on the Stack when Main() is called.
        // It only exists while the Main method is running.
        int itemPrice = 25;

        // 'isCheaperThanLimit' is a local 'bool' (Value Type).
        // It is pushed onto the Stack right next to 'itemPrice'.
        bool isCheaperThanLimit = false;

        Console.WriteLine($"Attempting to buy an item for: ${itemPrice}.");
        Console.WriteLine($"Current Global Balance: ${globalBalance}");


        // --- BLOCK 2: The Heap (Reference Types via Arrays) ---
        
        // 'shoppingCart' is an Array of integers.
        // The Array itself is a Reference Type.
        // 1. The variable 'shoppingCart' (the reference/address) lives on the Stack.
        // 2. The actual data [5, 10, 25] lives on the Heap.
        int[] shoppingCart = new int[] { 5, 10, 25 };

        Console.WriteLine("\nIterating through the shopping cart (Heap data):");
        
        // We use a foreach loop (Chapter 12) to read the data.
        foreach (int cartItem in shoppingCart)
        {
            Console.Write($"Item: ${cartItem} ");
        }
        Console.WriteLine(); // New line for formatting


        // --- BLOCK 3: Scope and Logic ---

        // We enter a new scope here. Variables declared inside these brackets
        // are LOCAL to this block.
        if (globalBalance >= itemPrice)
        {
            // 'transactionSuccess' is a local variable inside this 'if' block.
            // It is a Value Type (bool) on the Stack.
            bool transactionSuccess = true;

            // We modify the global variable defined at the top of the class.
            // This changes the value in the global scope.
            globalBalance = globalBalance - itemPrice;

            Console.WriteLine($"\nTransaction Successful! Remaining Balance: ${globalBalance}");
        }
        // 'transactionSuccess' is DESTROYED here. It falls out of scope.
        // The memory on the Stack used by 'transactionSuccess' is now free.

        // --- BLOCK 4: Variable Lifetime End ---
        
        // 'itemPrice' and 'shoppingCart' (the reference) are still valid here
        // because we are still inside the Main method.
        Console.WriteLine($"Final check: Item price was ${itemPrice}.");

    } // END OF MAIN METHOD
      // 1. 'itemPrice', 'isCheaperThanLimit', and 'shoppingCart' are DESTROYED from the Stack.
      // 2. The Array data [5, 10, 25] on the Heap is marked for Garbage Collection.
      // 3. 'globalBalance' stays alive (conceptually) because the program hasn't fully terminated yet.
}
