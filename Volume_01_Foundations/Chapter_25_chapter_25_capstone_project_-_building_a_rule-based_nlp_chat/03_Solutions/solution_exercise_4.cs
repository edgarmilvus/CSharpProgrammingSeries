
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

public class ChatBot
{
    private List<IResponseStrategy> _strategies;
    private Logger _logger;
    
    // Ch 2: Variable declaration (Field for state)
    // Ch 16: Class fields store state between method calls
    private int _consecutiveFailures;

    public ChatBot(Logger logger)
    {
        _strategies = new List<IResponseStrategy>();
        _logger = logger;
        _consecutiveFailures = 0; // Initialize to 0
    }

    public void AddStrategy(IResponseStrategy strategy)
    {
        _strategies.Add(strategy);
    }

    public string GetReply(string input)
    {
        string response = "";
        bool handled = false;

        foreach (IResponseStrategy strategy in _strategies)
        {
            if (strategy.CanHandle(input))
            {
                response = strategy.GetResponse(input);
                handled = true;
                break; // Exit loop once handled
            }
        }

        // Ch 6: if/else logic
        if (handled)
        {
            // Reset counter on success
            _consecutiveFailures = 0;
        }
        else
        {
            // Increment failure counter
            // Ch 4: Increment operator (++)
            _consecutiveFailures++;

            // Check if we have failed 3 or more times
            if (_consecutiveFailures >= 3)
            {
                response = "I'm having trouble understanding. Try asking about the weather or saying 'hello'.";
            }
            else
            {
                response = "I don't understand that command.";
            }
        }

        _logger.LogMessage(input, response);
        return response;
    }
}
