
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

namespace SimpleChatbot
{
    class Program
    {
        // The Main method is the entry point of our application
        static void Main(string[] args)
        {
            // 1. Define the rule: A keyword and the intended response.
            // We use 'string' variables to hold text data.
            string targetKeyword = "hours";
            string response = "We are open from 9 AM to 5 PM, Monday to Friday.";

            // 2. Start an infinite loop to keep the chatbot running.
            // We use a 'while' loop that checks if 'true' (always runs).
            while (true)
            {
                // 3. Prompt the user and capture their input.
                Console.Write("User: ");
                string userInput = Console.ReadLine();

                // 4. Check if the user wants to exit.
                // We use the '!' (not) operator and '==' (equality).
                if (userInput == "exit")
                {
                    break; // Exit the loop
                }

                // 5. Analyze the input.
                // We use the 'Contains' method (allowed from String methods) to find keywords.
                // To be safe, we convert both to lowercase so "Hours" matches "hours".
                if (userInput.ToLower().Contains(targetKeyword))
                {
                    // 6. Output the response if the rule matches.
                    Console.WriteLine("Bot: " + response);
                }
                else
                {
                    // 7. Fallback response if no rule matches.
                    Console.WriteLine("Bot: I'm sorry, I don't understand. Ask about our hours.");
                }
            }
        }
    }
}
