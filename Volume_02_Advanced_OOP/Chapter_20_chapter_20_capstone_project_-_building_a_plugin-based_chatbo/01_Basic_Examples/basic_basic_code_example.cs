
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

// Define a delegate type that represents a strategy for generating a response.
// This delegate takes a user input string and returns a response string.
public delegate string ResponseStrategy(string userInput);

public class SimpleChatbot
{
    // A dictionary to map keywords (or intents) to specific response strategies.
    // This acts as our plugin registry.
    private readonly Dictionary<string, ResponseStrategy> _strategies;

    public SimpleChatbot()
    {
        _strategies = new Dictionary<string, ResponseStrategy>();

        // Register a strategy using a Lambda Expression.
        // This lambda captures the specific logic for handling "order" queries.
        _strategies.Add("order", (input) => 
        {
            // In a real system, we might parse the order ID from the input.
            return "To check your order status, please provide your order ID.";
        });

        // Register another strategy for "return" queries.
        _strategies.Add("return", (input) => 
        {
            return "To process a return, please visit the returns page at our website.";
        });

        // Register a default strategy using a lambda for unmatched queries.
        _strategies.Add("default", (input) => 
        {
            return "I'm sorry, I don't understand that. Can you ask about an order or a return?";
        });
    }

    // Method to process user input and select the correct strategy.
    public string GetResponse(string userInput)
    {
        // Normalize input to lower case for easier matching.
        string normalizedInput = userInput.ToLower();

        // Find the first key in the dictionary that appears in the user's input.
        // This simulates a simple intent recognition system.
        string matchedKey = _strategies.Keys.FirstOrDefault(key => normalizedInput.Contains(key));

        // If no specific key is matched, use the default strategy.
        ResponseStrategy strategy = matchedKey != null ? _strategies[matchedKey] : _strategies["default"];

        // Execute the selected strategy delegate.
        return strategy(userInput);
    }
}

public class Program
{
    public static void Main()
    {
        // Instantiate the chatbot.
        var chatbot = new SimpleChatbot();

        // Simulate a conversation loop.
        Console.WriteLine("Chatbot: Hello! How can I help you today?");
        while (true)
        {
            Console.Write("User: ");
            string input = Console.ReadLine();

            if (input == "exit")
            {
                break;
            }

            // Get the response from the chatbot.
            string response = chatbot.GetResponse(input);
            Console.WriteLine($"Chatbot: {response}");
        }
    }
}
