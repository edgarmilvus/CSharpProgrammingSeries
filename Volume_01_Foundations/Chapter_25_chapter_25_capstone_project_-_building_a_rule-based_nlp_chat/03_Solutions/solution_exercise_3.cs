
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.IO; // Ch 24: Required for file operations

public class Logger
{
    private string _filePath = "chatlog.txt";

    public void LogMessage(string userMessage, string botResponse)
    {
        // Ch 3: String Interpolation ($) to format the log entry
        string logEntry = $"[{DateTime.Now}] User: {userMessage} | Bot: {botResponse}{Environment.NewLine}";
        
        // Ch 24: File.AppendAllText
        // This creates the file if missing, or adds to the end without deleting old data
        File.AppendAllText(_filePath, logEntry);
    }
}

// Updated ChatBot to use the Logger
public class ChatBot
{
    private List<IResponseStrategy> _strategies;
    private Logger _logger; // Field to hold the logger instance

    // Ch 18: Constructor
    public ChatBot(Logger logger)
    {
        _strategies = new List<IResponseStrategy>();
        _logger = logger; // Dependency Injection via Constructor
    }

    public void AddStrategy(IResponseStrategy strategy)
    {
        _strategies.Add(strategy);
    }

    public string GetReply(string input)
    {
        string response = "I'm not sure how to respond to that.";

        foreach (IResponseStrategy strategy in _strategies)
        {
            if (strategy.CanHandle(input))
            {
                response = strategy.GetResponse(input);
                break;
            }
        }

        // Call the logger to save the interaction
        _logger.LogMessage(input, response);

        return response;
    }
}

// Main Program Example
public class Program
{
    public static void Main()
    {
        // Instantiate the logger first
        Logger logger = new Logger();
        
        // Pass logger to ChatBot
        ChatBot bot = new ChatBot(logger);
        
        bot.AddStrategy(new GreetingStrategy());
        
        // ... rest of interaction loop ...
    }
}
