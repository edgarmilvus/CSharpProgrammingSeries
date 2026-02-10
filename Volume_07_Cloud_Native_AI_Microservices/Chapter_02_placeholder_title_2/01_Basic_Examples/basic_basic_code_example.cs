
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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace GreetingAgentMicroservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Configure services for dependency injection
            builder.Services.AddSingleton<IGreetingService, GreetingService>();
            builder.Services.AddControllers(); // For potential future endpoints
            
            var app = builder.Build();
            
            // Define a single endpoint for our greeting agent
            app.MapGet("/api/greet/{userName}", (string userName, IGreetingService greetingService) =>
            {
                var greeting = greetingService.GenerateGreeting(userName);
                return Results.Ok(new { Message = greeting, Timestamp = DateTime.UtcNow });
            });
            
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();
            
            // Run the application
            app.Run();
        }
    }

    // Interface for the greeting service (dependency inversion)
    public interface IGreetingService
    {
        string GenerateGreeting(string userName);
    }

    // Concrete implementation of the greeting service
    public class GreetingService : IGreetingService
    {
        private readonly List<string> _greetingTemplates = new()
        {
            "Hello, {0}! Welcome to our AI-powered platform.",
            "Hi {0}, great to see you today!",
            "Greetings, {0}! How can our AI assist you?"
        };

        public string GenerateGreeting(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("User name cannot be empty", nameof(userName));
            }

            // Simple business logic: select a random greeting template
            var random = new Random();
            var template = _greetingTemplates[random.Next(_greetingTemplates.Count)];
            
            // Format the greeting with the user's name
            return string.Format(template, userName);
        }
    }
}
