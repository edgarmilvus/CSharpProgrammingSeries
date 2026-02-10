
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
using System.Collections.Generic; // Required for List<T> (Chapter 20)
using System.Text.RegularExpressions; // Required for Regex (Chapter 25 Capstone Logic)

// ==========================================
// 1. THE STRATEGY INTERFACE
// ==========================================
// We define a contract that all "Rules" must follow.
// In the Strategy Pattern, this is the abstraction that allows
// us to swap behaviors dynamically.
public interface IResponseRule
{
    // This method checks if the rule applies to the user's input.
    // Returns true if it matches, false otherwise.
    bool IsMatch(string userInput);

    // This method generates the response if the rule matches.
    // Returns the chatbot's reply as a string.
    string GetResponse(string userInput);
}

// ==========================================
// 2. CONCRETE STRATEGIES (The Rules)
// ==========================================

// Rule 1: Greeting Rule
// Matches inputs like "hello", "hi", "hey".
public class GreetingRule : IResponseRule
{
    public bool IsMatch(string userInput)
    {
        // We use String methods (Chapter 23) to normalize the input.
        // ToLower() makes matching case-insensitive.
        // Contains() checks if the substring exists.
        string lowerInput = userInput.ToLower();
        
        return lowerInput.Contains("hello") || 
               lowerInput.Contains("hi") || 
               lowerInput.Contains("hey");
    }

    public string GetResponse(string userInput)
    {
        // Random is allowed (System namespace is implicit in most contexts, 
        // but we will stick to basic logic for determinism in this example).
        // We return a static response for this rule.
        return "Hello there! How can I help you with your C# learning today?";
    }
}

// Rule 2: Time Rule
// Matches inputs containing "time" or "clock".
// Uses Regex for pattern matching (Chapter 25 Capstone Logic).
public class TimeRule : IResponseRule
{
    public bool IsMatch(string userInput)
    {
        // Regex pattern: looks for the word "time" or "clock" anywhere.
        // RegexOptions.IgnoreCase handles capitalization.
        // We use Regex here because it's a specific tool for NLP patterns.
        return Regex.IsMatch(userInput, "time|clock", RegexOptions.IgnoreCase);
    }

    public string GetResponse(string userInput)
    {
        // DateTime.Now gives us the current system time.
        // We use String Interpolation (Chapter 3) to format the output.
        return $"It is currently {DateTime.Now.ToString("HH:mm")}.";
    }
}

// Rule 3: Math Calculation Rule
// Matches inputs starting with "calculate" or "math".
// Demonstrates parsing and arithmetic (Chapters 4, 5, 23).
public class MathRule : IResponseRule
{
    public bool IsMatch(string userInput)
    {
        string lowerInput = userInput.ToLower();
        return lowerInput.Contains("calculate") || lowerInput.Contains("math");
    }

    public string GetResponse(string userInput)
    {
        // We need to extract numbers from the string.
        // Strategy: Split the string by spaces (Chapter 23).
        string[] parts = userInput.Split(' ');

        // We look for the first two numbers in the sentence.
        double num1 = 0;
        double num2 = 0;
        bool foundFirst = false;
        bool foundSecond = false;

        // Iterating through the array (Chapter 12 - foreach).
        foreach (string part in parts)
        {
            // Try to parse the part into a number.
            // double.TryParse is safer than Parse because it doesn't crash on invalid input.
            // We use 'out' parameters (Chapter 14).
            double number;
            if (double.TryParse(part, out number))
            {
                if (!foundFirst)
                {
                    num1 = number;
                    foundFirst = true;
                }
                else if (!foundSecond)
                {
                    num2 = number;
                    foundSecond = true;
                    break; // Stop after finding two numbers (Chapter 10).
                }
            }
        }

        if (foundFirst && foundSecond)
        {
            // Perform basic arithmetic (Chapter 4).
            double result = num1 + num2;
            return $"The sum of {num1} and {num2} is {result}.";
        }

        return "I couldn't find two numbers to add. Try 'calculate 10 20'.";
    }
}

// Rule 4: Joke Rule
// Matches inputs containing "joke" or "funny".
public class JokeRule : IResponseRule
{
    public bool IsMatch(string userInput)
    {
        return userInput.ToLower().Contains("joke") || userInput.ToLower().Contains("funny");
    }

    public string GetResponse(string userInput)
    {
        // A simple hardcoded joke.
        return "Why do programmers prefer dark mode? Because light attracts bugs!";
    }
}

// Rule 5: Default/Fallback Rule
// This rule ALWAYS matches if no other rule did.
// It is the "catch-all" strategy.
public class DefaultRule : IResponseRule
{
    public bool IsMatch(string userInput)
    {
        // Always return true. This is the lowest priority rule.
        return true;
    }

    public string GetResponse(string userInput)
    {
        return "I'm not sure I understand. Try asking about time, math, or tell me a joke!";
    }
}

// ==========================================
// 3. THE RULE ENGINE (Dependency Injection)
// ==========================================
// This class manages the collection of rules.
// It does NOT know about specific rules like GreetingRule or TimeRule.
// It only knows about the IResponseRule interface.
public class RuleEngine
{
    // We store the rules in a List (Chapter 20).
    // We inject the dependencies via the constructor.
    private List<IResponseRule> _rules;

    // Constructor (Chapter 18).
    // Takes a list of rules as a parameter.
    public RuleEngine(List<IResponseRule> rules)
    {
        _rules = rules;
    }

    // The core logic method.
    public string ProcessInput(string input)
    {
        // Iterate through every rule in the list (Chapter 12).
        foreach (IResponseRule rule in _rules)
        {
            // Check if the rule matches the input.
            if (rule.IsMatch(input))
            {
                // Return the response from the matching rule.
                return rule.GetResponse(input);
            }
        }

        // This line should technically never be reached because 
        // DefaultRule is in the list and always returns true.
        return "I encountered an error processing your request.";
    }
}

// ==========================================
// 4. MAIN APPLICATION
// ==========================================
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- C# Rule-Based Chatbot Started ---");
        Console.WriteLine("Try saying 'Hello', 'What time is it?', 'Calculate 5 10', or 'Tell me a joke'.");
        Console.WriteLine("Type 'exit' to quit.");

        // DEPENDENCY INJECTION SETUP
        // We create instances of our concrete rules (Chapter 16).
        // Note: We use the 'new' keyword to instantiate classes.
        IResponseRule greeting = new GreetingRule();
        IResponseRule time = new TimeRule();
        IResponseRule math = new MathRule();
        IResponseRule joke = new JokeRule();
        IResponseRule fallback = new DefaultRule();

        // We create a list to hold these rules (Chapter 20).
        List<IResponseRule> ruleList = new List<IResponseRule>();

        // We add the rules to the list.
        // ORDER MATTERS! We add specific rules before the generic fallback.
        ruleList.Add(greeting);
        ruleList.Add(time);
        ruleList.Add(math);
        ruleList.Add(joke);
        ruleList.Add(fallback);

        // We inject the list into the RuleEngine.
        // The engine doesn't care what the rules are, just that they implement IResponseRule.
        RuleEngine engine = new RuleEngine(ruleList);

        // The Chat Loop (Chapter 8 - while loop).
        while (true)
        {
            Console.Write("\nUser: ");
            string input = Console.ReadLine();

            // Check for exit condition (Chapter 6).
            if (input.ToLower() == "exit")
            {
                break;
            }

            // Pass the input to the engine and get the response.
            string response = engine.ProcessInput(input);

            // Display the response (Chapter 1).
            Console.WriteLine($"Bot: {response}");
        }

        Console.WriteLine("Goodbye!");
    }
}
