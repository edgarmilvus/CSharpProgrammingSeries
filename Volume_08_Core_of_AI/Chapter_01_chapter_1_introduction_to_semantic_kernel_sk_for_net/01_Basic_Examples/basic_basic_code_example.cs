
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;

// 1. Setup and Configuration
// ---------------------------------------------------------
// In a real app, load this from appsettings.json or environment variables.
// For this example, we assume a local Ollama instance running 'phi3' or 'mistral'.
// If you don't have Ollama, replace Endpoint with Azure OpenAI or OpenAI details.
var builder = Kernel.CreateBuilder();
builder.AddOllamaChatCompletion(
    modelId: "phi3", 
    endpoint: new Uri("http://localhost:11434")
);

// Build the Kernel instance. This is the central orchestrator.
Kernel kernel = builder.Build();

// 2. Define the Native Plugin (The "Grounding" Layer)
// ---------------------------------------------------------
// This class represents native .NET code that the LLM can invoke.
// The [Description] attribute tells the LLM what the function does.
public class MoviePlugin
{
    // A hardcoded list of movies (simulating a database query).
    private readonly List<Movie> _movies = new()
    {
        new Movie("Inception", "Sci-Fi", 8.8),
        new Movie("The Shawshank Redemption", "Drama", 9.3),
        new Movie("Se7en", "Mystery", 8.6),
        new Movie("The Dark Knight", "Action", 9.0)
    };

    [KernelFunction, Description("Retrieves a list of movies based on a specific genre.")]
    public string GetMoviesByGenre([Description("The genre of the movie (e.g., Mystery, Drama)")] string genre)
    {
        var filteredMovies = _movies
            .Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!filteredMovies.Any())
            return $"No movies found for genre: {genre}";

        // Return a serialized JSON string so the LLM can parse it easily.
        return JsonSerializer.Serialize(filteredMovies, new JsonSerializerOptions { WriteIndented = true });
    }
}

// Simple record to structure our data
public record Movie(string Title, string Genre, double Rating);

// 3. Register the Plugin with the Kernel
// ---------------------------------------------------------
// We add our C# class to the kernel. The kernel now knows these functions exist.
kernel.Plugins.AddFromType<MoviePlugin>("MovieLibrary");

// 4. Define the Execution Plan (The "Orchestrator")
// ---------------------------------------------------------
// We use a Prompt Execution Settings object to tell the LLM it CAN use tools.
// This is the modern replacement for the older "Planner" class in SK.
var executionSettings = new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions // Automatically triggers the C# code if needed
};

// 5. The Interaction (User -> Kernel -> LLM -> Plugin)
// ---------------------------------------------------------
Console.WriteLine("User: I want to watch a mystery movie tonight.");
Console.WriteLine("System: Processing request...\n");

// The prompt sent to the LLM. 
// We instruct the LLM to act as an assistant and use the tools provided.
string prompt = "Suggest a mystery movie from the provided library.";

// Execute the kernel. 
// Behind the scenes:
// 1. The LLM receives the prompt + the description of 'MovieLibrary'.
// 2. The LLM decides 'GetMoviesByGenre' is needed with argument "Mystery".
// 3. Semantic Kernel invokes the C# method 'GetMoviesByGenre'.
// 4. The result (JSON data) is sent back to the LLM.
// 5. The LLM formats the JSON into a natural language response.
var result = await kernel.InvokePromptAsync(prompt, executionSettings);

// 6. Output
// ---------------------------------------------------------
Console.WriteLine($"Assistant: {result}");
