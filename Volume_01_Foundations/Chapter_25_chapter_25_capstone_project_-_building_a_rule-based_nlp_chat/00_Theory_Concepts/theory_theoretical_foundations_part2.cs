
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

// Source File: theory_theoretical_foundations_part2.cs
// Description: Theoretical Foundations
// ==========================================

public class ChatBotEngine
{
    // A dynamic collection to hold our rules (Chapter 20: List<T>)
    private List<ResponseRule> _rules;

    // Constructor initializes the list (Chapter 18, 20)
    public ChatBotEngine()
    {
        _rules = new List<ResponseRule>();
    }

    // Method to add rules dynamically
    public void AddRule(ResponseRule rule)
    {
        _rules.Add(rule);
    }

    // The core processing logic
    public string GetResponse(string userInput)
    {
        // Normalize input to lowercase for better matching
        // (We will simulate this by manually iterating and checking)
        string lowerInput = userInput.ToLower(); 

        // Iterate through rules using a foreach loop (Chapter 12)
        foreach (ResponseRule rule in _rules)
        {
            // Check if the rule's pattern exists in the input
            // We convert the rule pattern to lowercase too for comparison
            if (lowerInput.Contains(rule.Pattern.ToLower()))
            {
                return rule.Response;
            }
        }

        // Default response if no rule matches (Chapter 6: if/else)
        return "I'm not sure how to respond to that.";
    }
}
