
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

using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

// 1. Define the Domain Model
public class BlogPost
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
}

// 2. Define the EF Core DbContext
public class BlogContext : DbContext
{
    public DbSet<BlogPost> BlogPosts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=blog.db");
}

// 3. Define the LLM Plugin for Data Generation
public class DataGeneratorPlugin
{
    [KernelFunction("generate_blog_post")]
    [Description("Generates a single, realistic blog post object with title, content, author, and date.")]
    public BlogPost GenerateBlogPost()
    {
        // In a real scenario, this function would be decorated with [KernelFunction] 
        // and call an LLM. For this "Hello World" example, we simulate the LLM 
        // response to ensure the code is runnable without external API keys.
        return new BlogPost
        {
            Title = "The Future of AI in .NET",
            Content = "Artificial Intelligence is rapidly changing how we write software...",
            Author = "Jane Doe",
            PublishedDate = DateTime.UtcNow
        };
    }
}

// 4. Main Execution Logic
public class Program
{
    public static async Task Main(string[] args)
    {
        // A. Setup the Kernel (The Orchestrator)
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "demo-key")
            .Build();

        // B. Register the Plugin
        var generatorPlugin = new DataGeneratorPlugin();
        kernel.Plugins.AddFromObject(generatorPlugin, "DataGen");

        // C. Setup the Database
        using var context = new BlogContext();
        await context.Database.EnsureDeletedAsync(); // Clean slate for demo
        await context.Database.EnsureCreatedAsync();

        // D. Generate Data via Kernel
        Console.WriteLine("Generating synthetic data via LLM...");
        
        // Invoking the specific function defined in our plugin
        var result = await kernel.InvokeAsync(
            "DataGen", 
            "generate_blog_post"
        );

        // E. Parse and Seed
        // The Kernel returns a function result; we cast it to our domain object
        var syntheticPost = (BlogPost)result.Value;

        context.BlogPosts.Add(syntheticPost);
        await context.SaveChangesAsync();

        // F. Verify
        var savedPost = await context.BlogPosts.FirstAsync();
        Console.WriteLine($"Seeded: '{savedPost.Title}' by {savedPost.Author}");
    }
}
