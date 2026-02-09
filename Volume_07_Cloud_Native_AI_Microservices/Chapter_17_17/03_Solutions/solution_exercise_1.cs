
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

// Program.cs - .NET 8 Wrapper for Python Inference Service
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configure the Python process startup info
var pythonProcessInfo = new ProcessStartInfo
{
    FileName = "python3",
    Arguments = "inference_service.py", // The Python script
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    CreateNoWindow = true
};

var app = builder.Build();

// Global variable to track the Python subprocess
private static Process? _pythonProcess;

// Start the Python service on application startup
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        _pythonProcess = new Process { StartInfo = pythonProcessInfo };
        _pythonProcess.Start();
        
        // Log output for debugging
        _ = Task.Run(async () => 
        {
            while (!_pythonProcess.StandardOutput.EndOfStream)
            {
                var line = await _pythonProcess.StandardOutput.ReadLineAsync();
                Console.WriteLine($"[Python]: {line}");
            }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to start Python service: {ex.Message}");
        app.Lifetime.StopApplication();
    }
});

// Graceful shutdown: Kill the Python process when .NET app stops
app.Lifetime.ApplicationStopping.Register(() =>
{
    if (_pythonProcess != null && !_pythonProcess.HasExited)
    {
        _pythonProcess.Kill(true); // Kill process tree
        _pythonProcess.WaitForExit(5000); // Wait up to 5 seconds
    }
});

// Health Check Endpoint
app.MapGet("/healthz", (HttpContext context) =>
{
    if (_pythonProcess == null || _pythonProcess.HasExited)
    {
        context.Response.StatusCode = 503; // Service Unavailable
        return Results.Text("Python process is not running");
    }
    
    // Optional: Ping the python service to ensure it's responsive
    return Results.Ok("Healthy");
});

// Proxy Endpoint for Inference
app.MapPost("/classify", async (HttpContext context) =>
{
    // In a real scenario, you might forward the request body to the Python process
    // or invoke the Python script directly. 
    // For this wrapper, we assume the Python process exposes a local port (e.g., 5000)
    // and we proxy the request.
    
    using var client = new HttpClient();
    try 
    {
        // Forward request to the internal Python service
        var response = await client.PostAsync("http://localhost:5000/classify", 
            new StreamContent(context.Request.Body));
        
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content);
    }
    catch
    {
        return Results.StatusCode(503);
    }
});

app.Run();
