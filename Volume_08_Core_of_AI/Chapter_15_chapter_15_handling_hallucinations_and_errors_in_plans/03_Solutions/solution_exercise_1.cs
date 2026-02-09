
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Define the RecipePlan class
public class RecipePlan
{
    [JsonPropertyName("dishName")]
    public string DishName { get; set; } = string.Empty;

    [JsonPropertyName("ingredients")]
    public List<string> Ingredients { get; set; } = new();

    [JsonPropertyName("cookingTimeMinutes")]
    public int CookingTimeMinutes { get; set; }

    [JsonPropertyName("instructions")]
    public List<string> Instructions { get; set; } = new();
}

// Custom Exception
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

// 3. Validator Class
public class RecipeValidator
{
    // Interactive Challenge: Known Ingredients Dictionary
    private static readonly Dictionary<string, List<string>> KnownIngredients = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Pizza", new List<string> { "dough", "tomato sauce", "cheese", "pepperoni", "basil" } },
        { "Salad", new List<string> { "lettuce", "tomato", "cucumber", "dressing", "olive oil" } },
        { "Pasta", new List<string> { "pasta", "tomato", "garlic", "olive oil", "parmesan" } }
    };

    public async Task ValidateRecipeAsync(RecipePlan plan, string requestedDishType)
    {
        // 1. Logic Checks
        if (plan.CookingTimeMinutes <= 0)
            throw new ValidationException("Cooking time must be positive.");

        if (plan.Ingredients == null || plan.Ingredients.Count == 0)
            throw new ValidationException("Recipe must contain at least one ingredient.");

        // 2. Hallucination Check (Dish Name)
        if (!plan.DishName.Contains(requestedDishType, StringComparison.OrdinalIgnoreCase))
        {
            // In a real scenario, this might trigger a specific retry logic. 
            // For this exercise, we treat it as a validation failure.
            throw new ValidationException($"Dish name '{plan.DishName}' does not match requested type '{requestedDishType}'.");
        }

        // 3. Interactive Challenge: Cross-Reference Check
        if (KnownIngredients.TryGetValue(requestedDishType, out var validIngredients))
        {
            int hallucinatedCount = 0;
            foreach (var ing in plan.Ingredients)
            {
                // Simple substring match for demo purposes
                if (!validIngredients.Any(v => v.Contains(ing, StringComparison.OrdinalIgnoreCase)))
                {
                    hallucinatedCount++;
                }
            }

            double hallucinationRatio = (double)hallucinatedCount / plan.Ingredients.Count;
            if (hallucinationRatio > 0.5)
            {
                throw new ValidationException($"Over 50% of ingredients are unknown for {requestedDishType}. Potential hallucination.");
            }
        }

        // Simulate async work
        await Task.CompletedTask;
    }
}

// 2. Kernel Function for Recipe Generation
public class RecipePlanner
{
    private readonly Kernel _kernel;
    private readonly RecipeValidator _validator = new();

    public RecipePlanner(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<RecipePlan> GenerateValidatedRecipe(string requestedDishType)
    {
        int attempts = 0;
        string prompt = $"Generate a simple recipe for {requestedDishType}. Return ONLY raw JSON matching this schema: {{ dishName: string, ingredients: [string], cookingTimeMinutes: int, instructions: [string] }}.";
        
        // Refined prompt for retries
        string refinedPrompt = prompt;

        while (attempts < 3)
        {
            try
            {
                attempts++;
                Console.WriteLine($"Attempt {attempts} to generate recipe...");

                // Configure OpenAI for strict JSON mode
                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    ResponseFormat = "json_object",
                    Temperature = 0.1 // Low temp for consistency
                };

                // Execute LLM Call
                var result = await _kernel.InvokePromptAsync<RecipePlan>(refinedPrompt, new KernelArguments(executionSettings));

                if (result == null) throw new Exception("LLM returned null.");

                // Validate
                await _validator.ValidateRecipeAsync(result, requestedDishType);

                Console.WriteLine("Recipe validation successful.");
                return result;
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Validation failed: {ex.Message}");
                // Refine prompt based on error
                refinedPrompt = $"{prompt} \n\nPREVIOUS ERROR: {ex.Message}. Please correct this specific error and ensure the output is valid JSON.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Generation error: {ex.Message}");
                // Handle generic LLM errors (e.g., API timeout)
                if (attempts >= 3) throw;
            }
        }

        throw new Exception("Max attempts reached without valid recipe.");
    }
}

// Usage Example
public class Program
{
    public static async Task Main()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", "api-key") // Placeholder
            .Build();

        var planner = new RecipePlanner(kernel);
        
        try 
        {
            var recipe = await planner.GenerateValidatedRecipe("Pizza");
            Console.WriteLine(JsonSerializer.Serialize(recipe, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Final failure: {e.Message}");
        }
    }
}
