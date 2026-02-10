
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

// Project: InferenceApi.csproj
// <Project Sdk="Microsoft.NET.Sdk.Web">
//   <PropertyGroup>
//     <TargetFramework>net8.0</TargetFramework>
//   </PropertyGroup>
//   <ItemGroup>
//     <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="8.0.0" />
//   </ItemGroup>
// </Project>

// Program.cs
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

builder.Services.AddControllers();
var app = builder.Build();

// Simulate initialization state for Readiness Probe
var isInitialized = false;
Task.Run(async () => 
{
    await Task.Delay(2000); // Simulate startup time
    isInitialized = true;
});

app.UseAuthorization();

// 2. Heavy Workload Endpoint
app.MapPost("/api/inference", (InferenceRequest request) =>
{
    // Simulate Memory Pressure
    var bigArray = new byte[10 * 1024 * 1024]; // 10 MB allocation
    
    // Simulate CPU Intensive work (Leibniz series for Pi)
    // Adjust iterations based on request intensity
    int iterations = request.Intensity * 1000000; 
    double sum = 0.0;
    double sign = 1.0;

    for (int i = 0; i < iterations; i++)
    {
        sum += sign / (2 * i + 1);
        sign *= -1;
    }
    
    // Clear memory to simulate processing end
    bigArray = null; 
    GC.Collect(); 

    double pi = 4.0 * sum;
    return Results.Ok(new { PiApproximation = pi, Iterations = iterations });
});

// 3. Health Endpoints
app.MapGet("/health/ready", () => 
    isInitialized 
        ? Results.Ok("Ready") 
        : Results.StatusCode(503));

app.MapGet("/health/live", () => 
    Results.Ok("Alive"));

app.Run();

public record InferenceRequest(int Intensity);
