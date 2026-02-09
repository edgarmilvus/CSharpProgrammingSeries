
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

public static class ContextManager
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions 
    { 
        WriteIndented = true, 
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
    };

    /// <summary>
    /// Saves the context to a file.
    /// </summary>
    /// <param name="context">The conversation context to save.</param>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <param name="preSaveHook">A delegate (lambda) executed before serialization to modify state.</param>
    public static void Save(ConversationContext context, string filePath, Action<ConversationContext> preSaveHook = null)
    {
        try
        {
            // Execute the delegate if provided. 
            // This allows us to inject logic like "Remove sensitive data" without changing this method.
            preSaveHook?.Invoke(context);

            string jsonString = JsonSerializer.Serialize(context, Options);
            File.WriteAllText(filePath, jsonString);
            
            Console.WriteLine($"Context saved to {filePath}");
        }
        catch (Exception ex)
        {
            // In production AI apps, we must log this, not just print.
            Console.WriteLine($"Failed to save context: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads the context from a file.
    /// </summary>
    public static ConversationContext Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("No saved conversation found.", filePath);
        }

        try
        {
            string jsonString = File.ReadAllText(filePath);
            var context = JsonSerializer.Deserialize<ConversationContext>(jsonString, Options);
            
            // Post-load validation (e.g., checking if the schema version is compatible)
            if (context.Version != "1.0")
            {
                Console.WriteLine("Warning: Loaded context version mismatch. Migration may be required.");
            }

            return context;
        }
        catch (JsonException jsonEx)
        {
            // Data corruption scenario
            Console.WriteLine("Corrupted data file. Unable to parse JSON.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading context: {ex.Message}");
            throw;
        }
    }
}
