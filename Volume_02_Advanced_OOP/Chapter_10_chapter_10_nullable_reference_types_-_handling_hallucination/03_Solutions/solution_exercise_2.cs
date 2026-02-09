
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

public class ResponseValidator
{
    public Option<string> ValidateResponse(string rawInput)
    {
        // 1. Check for null
        if (rawInput == null) return Option<string>.None();

        // 2. Check for empty or whitespace (treating whitespace-only as empty)
        if (string.IsNullOrWhiteSpace(rawInput)) return Option<string>.None();

        // 3. Check for simulated hallucination markers
        if (rawInput.Contains("hallucination", StringComparison.OrdinalIgnoreCase))
        {
            return Option<string>.None();
        }

        // 4. Return valid data wrapped in Some (trimmed for cleanliness)
        return Option<string>.Some(rawInput.Trim());
    }

    public void ProcessUserQuery(string query)
    {
        Option<string> result = ValidateResponse(query);
        
        // Use Match to handle the result safely
        result.Match(
            ifSome: response => Console.WriteLine($"AI: {response}"),
            ifNone: () => Console.WriteLine("System: No valid response generated (Null/Empty/Hallucination detected).")
        );
    }
}
