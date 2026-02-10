
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
using System.Collections.Generic;

// 1. Define the Interface (Assumed from Chapter context)
public interface IResponseStrategy
{
    bool CanHandle(string input);
    string GetResponse(string input);
}

// 2. The new GreetingStrategy Class
public class GreetingStrategy : IResponseStrategy
{
    // Ch 14: Method Parameters and Return Values
    public bool CanHandle(string input)
    {
        // Ch 23: String.Split to separate words
        string[] words = input.Split(' ');

        // Ch 12: foreach loops to iterate over the array
        foreach (string word in words)
        {
            // Ch 6: Comparison (==) and Ch 7: Logical OR (||)
            if (word == "hello" || word == "hi" || word == "hey" || word == "start")
            {
                return true;
            }
        }
        return false;
    }

    public string GetResponse(string input)
    {
        // Ch 3: String Interpolation ($)
        return "Hello there! How can I help you today?";
    }
}

// 3. The ChatBot Class (Modified to accept strategies)
public class ChatBot
{
    // Ch 20: List<T> to hold the strategies
    private List<IResponseStrategy> _strategies;

    public ChatBot()
    {
        _strategies = new List<IResponseStrategy>();
    }

    // Method to inject strategies (Dependency Injection)
    public void AddStrategy(IResponseStrategy strategy)
    {
        _strategies.Add(strategy);
    }

    public string GetReply(string input)
    {
        // Ch 12: foreach loop to check strategies
        foreach (IResponseStrategy strategy in _strategies)
        {
            // Ch 6: if statement
            if (strategy.CanHandle(input))
            {
                return strategy.GetResponse(input);
            }
        }

        return "I'm not sure how to respond to that.";
    }
}

// 4. Main Program to demonstrate usage
public class Program
{
    public static void Main()
    {
        ChatBot bot = new ChatBot();

        // Injecting the new strategy
        bot.AddStrategy(new GreetingStrategy());

        // Simple loop to test
        while (true)
        {
            // Ch 5: Console.ReadLine
            string input = Console.ReadLine();

            if (input == "exit") break;

            string response = bot.GetReply(input);
            // Ch 1: Console.WriteLine
            Console.WriteLine(response);
        }
    }
}
