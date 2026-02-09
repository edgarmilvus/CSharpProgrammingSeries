
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

namespace AI_Cooking_Assistant
{
    // 1. The Option Container
    // Represents a value that may or may not be present.
    // 'T' is the type of the value (e.g., string, Recipe).
    public class Option<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        // Constructor for a value that exists
        public Option(T value)
        {
            _value = value;
            _hasValue = true;
        }

        // Constructor for an empty state (None)
        public Option()
        {
            _value = default(T); // Default value for type T
            _hasValue = false;
        }

        // Helper property to check existence
        public bool HasValue => _hasValue;

        // Helper property to safely retrieve value
        public T Value
        {
            get
            {
                if (!_hasValue)
                    throw new InvalidOperationException("Cannot access Value on an empty Option.");
                return _value;
            }
        }
    }

    // 2. Static Factory for cleaner syntax
    public static class Option
    {
        public static Option<T> Some<T>(T value) => new Option<T>(value);
        public static Option<T> None<T>() => new Option<T>();
    }

    // 3. The AI Service Simulation
    public class AIRecipeService
    {
        // Simulates an API call that might return null or empty
        public Option<string> GetRecipeSuggestion(string weather)
        {
            if (weather == "Rainy")
            {
                // Simulate successful generation
                return Option.Some("Chicken Soup with extra ginger");
            }
            else if (weather == "Storm")
            {
                // Simulate an empty response (hallucination resulted in nothing)
                return Option.None<string>();
            }
            else
            {
                // Simulate a null reference / timeout
                return null; // This is a potential failure point we need to handle
            }
        }
    }

    // 4. Main Execution
    class Program
    {
        static void Main(string[] args)
        {
            AIRecipeService service = new AIRecipeService();

            // Case 1: Valid Response
            Option<string> rainyRecipe = service.GetRecipeSuggestion("Rainy");
            ProcessRecipe(rainyRecipe);

            // Case 2: Empty Response (Hallucination)
            Option<string> stormRecipe = service.GetRecipeSuggestion("Storm");
            ProcessRecipe(stormRecipe);

            // Case 3: Null Response (Timeout)
            // Note: In a strictly typed system, we might wrap this return value 
            // immediately in an Option, but for this example, we assume the 
            // service might return null directly.
            Option<string> sunnyRecipe = service.GetRecipeSuggestion("Sunny");
            ProcessRecipe(sunnyRecipe);
        }

        // The handler logic that prevents crashes
        static void ProcessRecipe(Option<string> recipeOption)
        {
            // Defensive check: Is the container itself null?
            if (recipeOption == null)
            {
                Console.WriteLine("Error: The service returned a null reference.");
                return;
            }

            // Check if the container holds a value
            if (recipeOption.HasValue)
            {
                Console.WriteLine($"Success: Here is your recipe -> {recipeOption.Value}");
            }
            else
            {
                Console.WriteLine("Notice: The AI generated an empty response. Please try rephrasing.");
            }
        }
    }
}
