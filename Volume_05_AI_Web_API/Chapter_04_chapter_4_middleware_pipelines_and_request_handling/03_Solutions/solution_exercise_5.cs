
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Dependencies (Options, Middleware classes, Controllers)
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection(ApiKeyOptions.SectionName));
builder.Services.AddScoped<ApiKeyValidationMiddleware>();
builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();
// ... other registrations

var app = builder.Build();

// 2. Pipeline Configuration (Order is Critical)

// Priority 1: Exception Handling
// Catches any crashes in subsequent middleware (e.g., HSTS or Auth logic).
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Priority 2: Security (HTTPS/HSTS)
// Ensures the connection is secure before processing sensitive data.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// Priority 3: Authentication
// Validates identity before allowing access to logging or inference resources.
app.UseMiddleware<ApiKeyValidationMiddleware>();

// Priority 4: Auditing/Logging
// Logs the request/response payload. Placed after auth so we can log who made the request.
app.UseMiddleware<AiTrafficLoggingMiddleware>();

// Priority 5: Performance (Streaming)
// Optimizes the response stream. Placed before routing to ensure it wraps the controller response.
app.UseMiddleware<StreamingOptimizedMiddleware>();

// Priority 6: Routing
// Maps requests to specific Controller actions.
app.MapControllers();

app.Run();
