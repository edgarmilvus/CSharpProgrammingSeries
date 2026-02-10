
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

// File: Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog for JSON structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonTextFormatter()) // Outputs JSON to console
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add services (Controllers)
builder.Services.AddControllers();

var app = builder.Build();

// 3. Middleware: Request Logging & Stopwatch
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    // Log incoming request details
    using (LogContext.PushProperty("RequestMethod", context.Request.Method))
    using (LogContext.PushProperty("RequestPath", context.Request.Path))
    using (LogContext.PushProperty("QueryString", context.Request.QueryString.Value))
    {
        logger.LogInformation("Incoming Request");
        
        var stopwatch = Stopwatch.StartNew();
        await next.Invoke(); // Process the request
        stopwatch.Stop();

        // Log total request latency
        using (LogContext.PushProperty("TotalRequestLatencyMs", stopwatch.ElapsedMilliseconds))
        {
            logger.LogInformation("Request completed");
        }
    }
});

app.MapControllers();

app.Run();

// File: Controllers/ClassifyController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace TelemetryApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassifyController : ControllerBase
{
    private readonly ILogger<ClassifyController> _logger;

    public ClassifyController(ILogger<ClassifyController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Classify([FromBody] ClassificationRequest request)
    {
        const string modelVersion = "v1.2.0";
        var inputLength = request.Text?.Length ?? 0;

        // 4. Structured logging with scopes
        using (LogContext.PushProperty("ModelVersion", modelVersion))
        using (LogContext.PushProperty("InputLength", inputLength))
        {
            _logger.LogInformation("Starting model inference");

            var stopwatch = Stopwatch.StartNew();
            
            // Simulate model inference
            await Task.Delay(new Random().Next(50, 200)); 
            
            stopwatch.Stop();

            using (LogContext.PushProperty("InferenceDurationMs", stopwatch.ElapsedMilliseconds))
            {
                _logger.LogInformation("Model inference completed");
            }

            return Ok(new { Result = "Positive", Model = modelVersion });
        }
    }
}

public record ClassificationRequest(string Text);
