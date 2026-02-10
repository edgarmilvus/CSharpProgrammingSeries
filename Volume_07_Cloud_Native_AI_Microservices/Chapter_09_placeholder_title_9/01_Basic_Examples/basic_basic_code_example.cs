
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

// ProductRecommendationAgent.cs
// This single file contains a fully self-contained ASP.NET Core web API.
// It defines a microservice that acts as an AI agent to provide product recommendations.

using Microsoft.AspNetCore.Builder;         // For configuring the web application pipeline.
using Microsoft.AspNetCore.Mvc;             // For attributes like [HttpGet] and [FromServices].
using Microsoft.Extensions.DependencyInjection; // For the dependency injection container.
using Microsoft.Extensions.Hosting;         // For the application lifetime (IHost).
using System.Collections.Generic;           // For using List<T>.
using System.Linq;                          // For using LINQ's .FirstOrDefault().
using System.Text.Json;                     // For JSON serialization options.
using System.Text.Json.Serialization;       // For [JsonInclude] attribute.

// 1. **Domain Model Definition**: Represents the core data structure for our products.
// This is a simple record to hold product information. Records are immutable by default,
// which is excellent for preventing accidental state changes in a distributed system.
public record Product(
    int Id,
    string Name,
    string Category,
    double Price
);

// 2. **Data Abstraction**: Defines a contract for fetching product data.
// By depending on an interface, we decouple our agent's logic from the concrete data source.
// This is a key principle of microservices, allowing us to swap implementations
// (e.g., from an in-memory list to a database) without changing the agent's core logic.
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetByIdAsync(int id);
}

// 3. **Concrete Data Source**: A mock implementation of the repository.
// In a real-world scenario, this would be a service that queries a database,
// another microservice, or an external API.
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new()
    {
        new Product(1, "Quantum Laptop", "Electronics", 1200.00),
        new Product(2, "ErgoChair Pro", "Furniture", 350.00),
        new Product(3, "AI-Powered Mouse", "Electronics", 75.50),
        new Product(4, "Standing Desk", "Furniture", 450.00),
        new Product(5, "4K Monitor", "Electronics", 600.00)
    };

    public Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        // Asynchronously return the list of products.
        return Task.FromResult(_products.AsEnumerable());
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        // Asynchronously find a product by its ID.
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }
}

// 4. **AI Agent Logic**: The core "brain" of our microservice.
// This class contains the business logic for generating recommendations.
// It's registered in the DI container, making it available to our controllers.
public class RecommendationAgent
{
    private readonly IProductRepository _repository;

    // The constructor uses Dependency Injection to get an instance of the repository.
    // This is known as "Constructor Injection" and is a standard pattern.
    public RecommendationAgent(IProductRepository repository)
    {
        _repository = repository;
    }

    // This method encapsulates the recommendation algorithm.
    // For this "Hello World" example, the logic is simple:
    // Find the product and recommend another product from the same category.
    // In a real AI agent, this could involve a machine learning model inference call.
    public async Task<Product?> GetRecommendationAsync(int forProductId)
    {
        var sourceProduct = await _repository.GetByIdAsync(forProductId);
        if (sourceProduct == null) return null;

        var allProducts = await _repository.GetAllProductsAsync();

        // A simple recommendation logic: find another product in the same category.
        return allProducts
            .Where(p => p.Category == sourceProduct.Category && p.Id != sourceProduct.Id)
            .FirstOrDefault();
    }
}

// 5. **API Controller**: The public-facing entry point for our microservice.
// This class defines the HTTP endpoints that external clients (like a web frontend) can call.
[ApiController]
[Route("api/[controller]")] // Sets the base route to "/api/recommendation"
public class RecommendationController : ControllerBase
{
    private readonly RecommendationAgent _agent;

    // Constructor injection for the agent.
    public RecommendationController(RecommendationAgent agent)
    {
        _agent = agent;
    }

    // Defines an HTTP GET endpoint: e.g., /api/recommendation/1
    // This endpoint takes a product ID as a route parameter.
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetRecommendation(int productId)
    {
        var recommendedProduct = await _agent.GetRecommendationAsync(productId);

        if (recommendedProduct == null)
        {
            // If no recommendation is found, return a 404 Not Found response.
            return NotFound($"No recommendation found for product ID {productId}.");
        }

        // If a recommendation is found, return it as a 200 OK response with the JSON body.
        return Ok(recommendedProduct);
    }
}

// 6. **Application Entry Point**: The main program that builds and runs the web host.
public class Program
{
    public static async Task Main(string[] args)
    {
        // Create a builder for the web application.
        var builder = WebApplication.CreateBuilder(args);

        // Configure services for dependency injection.
        // This is the "composition root" where we wire up our dependencies.
        builder.Services.AddControllers(); // Adds MVC controllers to the DI container.

        // Register our custom services.
        // We use Scoped lifetime because we want a new repository/agent instance per HTTP request.
        // This is important for services that hold state (though ours don't).
        builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();
        builder.Services.AddScoped<RecommendationAgent>();

        // Build the application.
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // This sets up how incoming requests are handled.
        app.UseRouting(); // Enables routing for the application.

        // Map the controller routes to the endpoints.
        app.MapControllers();

        // Launch the application.
        // This will start an HTTP listener (by default on http://localhost:5000 and https://localhost:5001).
        // The application will run until it is shut down (e.g., by pressing Ctrl+C).
        Console.WriteLine("Recommendation Agent Microservice is starting...");
        Console.WriteLine("Try navigating to: http://localhost:5000/api/recommendation/1");
        await app.RunAsync();
    }
}
