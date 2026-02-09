
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;

namespace AdvancedOOP.NullableReferenceTypes
{
    // Generic Option/Maybe pattern implementation
    // This encapsulates the possibility of absence without using null references
    public class Option<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        // Private constructor - use static methods to create instances
        private Option(T value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        // Static factory method for Some (value present)
        public static Option<T> Some(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Cannot create Option.Some with null value");
            return new Option<T>(value, true);
        }

        // Static factory method for None (no value)
        public static Option<T> None()
        {
            return new Option<T>(default(T), false);
        }

        // Check if option contains a value
        public bool HasValue => _hasValue;

        // Safely retrieve value (throws if no value)
        public T Value
        {
            get
            {
                if (!_hasValue)
                    throw new InvalidOperationException("Cannot access value of Option.None");
                return _value;
            }
        }

        // Safe value retrieval with fallback
        public T ValueOrDefault(T defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }
    }

    // AI Response Model with nullable reference handling
    public class AIResponse
    {
        public string Content { get; }
        public ResponseType Type { get; }
        public DateTime Timestamp { get; }

        public AIResponse(string content, ResponseType type)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Type = type;
            Timestamp = DateTime.Now;
        }
    }

    public enum ResponseType
    {
        Valid,
        Empty,
        Hallucination
    }

    // Main application simulating AI response handling
    public class AIResponseProcessor
    {
        private readonly Random _random;

        public AIResponseProcessor()
        {
            _random = new Random();
        }

        // Simulate AI model generating a response
        // Returns Option<AIResponse> instead of nullable reference
        public Option<AIResponse> GenerateAIResponse(string query)
        {
            // Simulate different response scenarios
            int scenario = _random.Next(1, 4);

            if (scenario == 1)
            {
                // Valid response scenario
                string validContent = $"Processed query: '{query}'. This is a coherent response.";
                return Option<AIResponse>.Some(new AIResponse(validContent, ResponseType.Valid));
            }
            else if (scenario == 2)
            {
                // Empty response scenario (hallucination risk)
                return Option<AIResponse>.Some(new AIResponse("", ResponseType.Empty));
            }
            else
            {
                // Hallucination scenario
                string hallucinationContent = $"I know everything about '{query}' but wait... " +
                    "The quantum flux capacitor suggests that pineapples are actually databases. " +
                    "This is definitely correct information.";
                return Option<AIResponse>.Some(new AIResponse(hallucinationContent, ResponseType.Hallucination));
            }
        }

        // Process response with fallback strategies
        public string ProcessResponse(Option<AIResponse> responseOption, string originalQuery)
        {
            if (!responseOption.HasValue)
            {
                return "ERROR: No response received from AI model.";
            }

            AIResponse response = responseOption.Value;

            switch (response.Type)
            {
                case ResponseType.Valid:
                    return $"SUCCESS: {response.Content}";

                case ResponseType.Empty:
                    // Fallback strategy 1: Retry with modified query
                    string modifiedQuery = originalQuery + " (please provide detailed response)";
                    Option<AIResponse> retryResponse = GenerateAIResponse(modifiedQuery);
                    
                    if (retryResponse.HasValue && retryResponse.Value.Type == ResponseType.Valid)
                    {
                        return $"RETRY SUCCESS: {retryResponse.Value.Content}";
                    }
                    else
                    {
                        return "FALLBACK: The AI model returned an empty response. Please try again with a different query.";
                    }

                case ResponseType.Hallucination:
                    // Fallback strategy 2: Detect and handle hallucinations
                    if (ContainsHallucinationTriggers(response.Content))
                    {
                        return "HALLUCINATION DETECTED: The AI model provided nonsensical information. " +
                               "Please verify the response or try rephrasing your query.";
                    }
                    return $"WARNING: {response.Content} (Verify this information)";

                default:
                    return "UNKNOWN: Unexpected response type.";
            }
        }

        // Helper method to detect hallucination triggers
        private bool ContainsHallucinationTriggers(string content)
        {
            // Simple pattern matching without LINQ or lambda
            string[] triggers = { "quantum flux", "pineapples are actually", "definitely correct" };
            
            foreach (string trigger in triggers)
            {
                if (content.IndexOf(trigger, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            
            return false;
        }

        // Main processing pipeline
        public void RunSimulation(string userQuery)
        {
            Console.WriteLine($"=== AI Response Processing Simulation ===");
            Console.WriteLine($"User Query: \"{userQuery}\"");
            Console.WriteLine();

            // Generate initial response
            Option<AIResponse> initialResponse = GenerateAIResponse(userQuery);
            
            // Process with fallback strategies
            string result = ProcessResponse(initialResponse, userQuery);
            
            Console.WriteLine($"Result: {result}");
            Console.WriteLine();
        }
    }

    // Advanced Option extension methods for chaining operations
    public static class OptionExtensions
    {
        // Map operation: transform value if present
        public static Option<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> transform)
        {
            if (!option.HasValue)
                return Option<TResult>.None();
            
            return Option<TResult>.Some(transform(option.Value));
        }

        // Bind operation: transform to another Option
        public static Option<TResult> Bind<T, TResult>(this Option<T> option, Func<T, Option<TResult>> transform)
        {
            if (!option.HasValue)
                return Option<TResult>.None();
            
            return transform(option.Value);
        }

        // Match operation: handle both cases with separate functions
        public static TResult Match<T, TResult>(this Option<T> option, 
            Func<T, TResult> someFunc, 
            Func<TResult> noneFunc)
        {
            return option.HasValue ? someFunc(option.Value) : noneFunc();
        }
    }

    // Demonstration of advanced nullable reference handling
    public class AdvancedNullableDemo
    {
        public static void Execute()
        {
            Console.WriteLine("=== Advanced Nullable Reference Types Demo ===");
            Console.WriteLine();

            // Create processor instance
            AIResponseProcessor processor = new AIResponseProcessor();

            // Test multiple scenarios
            string[] testQueries = {
                "Explain quantum computing",
                "What is the capital of France?",
                "Describe the history of AI"
            };

            foreach (string query in testQueries)
            {
                processor.RunSimulation(query);
                Console.WriteLine("---");
            }

            // Demonstrate Option chaining without lambda (using explicit methods)
            Console.WriteLine("=== Option Pattern Chaining Demo ===");
            DemonstrateOptionChaining();
        }

        private static void DemonstrateOptionChaining()
        {
            // Create sample options
            Option<int> someNumber = Option<int>.Some(42);
            Option<int> noNumber = Option<int>.None();

            // Process some number
            Console.WriteLine("Processing Some(42):");
            ProcessNumberOption(someNumber);

            Console.WriteLine();

            // Process none
            Console.WriteLine("Processing None:");
            ProcessNumberOption(noNumber);
        }

        private static void ProcessNumberOption(Option<int> numberOption)
        {
            // Use Match to handle both cases explicitly
            string result = numberOption.Match(
                someValue => $"Value is {someValue}, doubled is {someValue * 2}",
                () => "No value available"
            );

            Console.WriteLine($"  {result}");
        }
    }

    // Main program entry point
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // Run the main simulation
                AdvancedNullableDemo.Execute();

                Console.WriteLine();
                Console.WriteLine("=== Simulation Complete ===");
                Console.WriteLine("Key takeaways:");
                Console.WriteLine("1. Option<T> pattern eliminates null reference exceptions");
                Console.WriteLine("2. Fallback strategies handle empty/hallucinated responses");
                Console.WriteLine("3. Static analysis ensures null-safety at compile time");
                Console.WriteLine("4. Generic implementations work with any data type");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}
