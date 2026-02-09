
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

using AiGrpcDemo;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// 1. Define the implementation of the gRPC service
public class ModelServiceImpl : ModelService.ModelServiceBase
{
    // Override the generated method from the base class
    public override Task<PredictionResponse> Predict(PredictionRequest request, ServerCallContext context)
    {
        // Simulate AI inference logic
        string result = $"Processed input '{request.InputText}' using model '{request.ModelName}'";
        
        // Return the response object wrapped in a Task
        return Task.FromResult(new PredictionResponse
        {
            Result = result,
            ConfidenceScore = 0.98 // Mock confidence
        });
    }
}

// 2. Configure the ASP.NET Core host
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddGrpc(); // Enable gRPC

var app = builder.Build();

// 3. Map the gRPC endpoint
app.MapGrpcService<ModelServiceImpl>();

// 4. Configure the HTTP pipeline
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
