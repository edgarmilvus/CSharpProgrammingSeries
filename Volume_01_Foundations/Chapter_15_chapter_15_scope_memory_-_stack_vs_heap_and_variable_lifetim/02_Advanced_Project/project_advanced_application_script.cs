
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;

namespace ArcadeScoreTracker
{
    class Program
    {
        // Global Scope: These variables exist for the entire life of the program.
        // They are stored on the Heap (as part of the Class object) or Static memory.
        static string arcadeName = "Neon Arcade";
        static int totalPlayers = 5;

        static void Main(string[] args)
        {
            // 1. STACK ALLOCATION: 'playerScores' is an array of integers.
            // The array object itself is a reference type (Heap), but the integers it holds 
            // are value types. In this beginner context, we treat the array as a fixed block of memory.
            int[] playerScores = new int[totalPlayers];

            Console.WriteLine($"Welcome to {arcadeName}!");
            Console.WriteLine("Please enter scores for {0} players.", totalPlayers);
            Console.WriteLine("--------------------------------------------------");

            // 2. SCOPE & LIFETIME LOOP:
            // The variable 'i' exists only inside this for-loop. 
            // When the loop finishes, 'i' is destroyed from the stack.
            for (int i = 0; i < totalPlayers; i++)
            {
                // We pass the specific index and the array reference to a helper method.
                // This demonstrates passing references between scopes.
                PromptForScore(playerScores, i);
            }

            Console.WriteLine("\nCalculating Statistics...");
            
            // 3. METHOD CALLS & RETURN VALUES:
            // We pass the array (reference) to methods. 
            // The methods calculate values and return them to the stack.
            int highestScore = GetHighestScore(playerScores);
            double averageScore = CalculateAverage(playerScores);

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Highest Score: {highestScore}");
            Console.WriteLine($"Average Score: {averageScore:F2}");
            Console.WriteLine("--------------------------------------------------");

            // 4. VARIABLE LIFETIME ENDS:
            // When Main finishes, 'playerScores', 'highestScore', and 'averageScore' 
            // go out of scope. The Garbage Collector will eventually reclaim the memory 
            // used by the array object on the heap.
        }

        // METHOD SCOPE:
        // This method has its own stack frame. 
        // Parameters 'scores' and 'index' are local to this method.
        static void PromptForScore(int[] scores, int index)
        {
            bool isValidInput = false;
            int score = 0;

            // 5. LOOP & LOGIC:
            // We use a while loop to ensure the user enters valid data.
            while (!isValidInput)
            {
                Console.Write($"Enter score for Player {index + 1}: ");
                string input = Console.ReadLine(); // 'input' is a string (Reference Type on Heap)

                // 6. TYPE CONVERSION & EXCEPTION HANDLING SIMULATION:
                // We cannot use try-catch yet (future concept), so we check manually.
                // int.Parse converts the string to an int. If it fails, it throws an error 
                // (which we can't catch, so we rely on valid input or simple logic checks).
                
                // Simple validation: Check if the string is empty or contains non-digits (basic check).
                // Note: Without advanced libraries, we rely on the user being nice, 
                // or we use int.TryParse (not allowed yet). 
                // For this script, we assume the user enters numbers, but we simulate a check.
                
                if (input.Length > 0)
                {
                    score = int.Parse(input); // Conversion happens here. 'score' is on the stack.
                    isValidInput = true;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
            }

            // 7. ARRAY INDEXING:
            // We modify the array located on the Heap by accessing it via the reference 'scores'.
            scores[index] = score;

            // 8. LOCAL VARIABLE DESTRUCTION:
            // When this method returns, 'input', 'isValidInput', and 'score' are popped off the stack.
            // The string 'input' (on the heap) becomes eligible for Garbage Collection if no other references exist.
        }

        // METHOD SCOPE:
        static int GetHighestScore(int[] scores)
        {
            // 9. INITIALIZATION ON STACK:
            // 'max' is a value type. It is created on the stack within this method's frame.
            // We initialize it to the first score in the array.
            int max = scores[0];

            // 10. FOREACH LOOP (Iterating Arrays):
            // 'score' is a local variable created for each iteration.
            // It copies the value from the array.
            foreach (int score in scores)
            {
                // 11. COMPARISON LOGIC:
                if (score > max)
                {
                    max = score; // Assignment to stack variable.
                }
            }

            // 12. RETURN VALUE:
            // The value of 'max' is copied to the return slot.
            // When the method ends, the stack frame (including 'max') is destroyed.
            return max;
        }

        // METHOD SCOPE:
        static double CalculateAverage(int[] scores)
        {
            // 13. ACCUMULATOR ON STACK:
            int sum = 0;
            
            // 14. FOR LOOP (Counted Iteration):
            for (int i = 0; i < scores.Length; i++)
            {
                // 15. ARITHMETIC:
                // 'sum' is updated. This happens on the stack.
                sum = sum + scores[i];
            }

            // 16. TYPE CONVERSION (IMPLICIT):
            // We must convert 'sum' (int) to double to get a decimal result.
            // This creates a new double value on the stack.
            double average = (double)sum / scores.Length;

            // 17. RETURN:
            return average;
        }
    }
}
