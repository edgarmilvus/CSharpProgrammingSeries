
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

// File: Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Setup OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

builder.Services.AddControllers();

var app = builder.Build();

// 1. Global Exception Handling Middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var activity = Activity.Current;

        // 3. Capture Exception in Telemetry (Span)
        if (activity != null)
        {
            activity.SetStatus(ActivityStatusCode.Error, "An error occurred");
            activity.RecordException(exception!);
        }

        // 3. Structured Logging
        logger.LogError(exception, "Unhandled exception occurred during request.");

        // 4. Generic Response to Client
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("An internal error occurred. Please try again later.");
    });
});

// Simulate ModelNotLoadedException
public class ModelNotLoadedException : Exception 
{
    public ModelNotLoadedException(string message) : base(message) { }
}

app.MapControllers();

app.Run();

// File: Controllers/ClassifyController.cs
[ApiController]
[Route("api/[controller]")]
public class ClassifyController : ControllerBase
{
    private readonly ILogger<ClassifyController> _logger;

    public ClassifyController(ILogger<ClassifyController> logger) => _logger = logger;

    [HttpPost]
    public IActionResult Classify([FromBody] ClassificationRequest request)
    {
        // 5. Simulate specific exception
        if (request.Text.Contains("fail"))
        {
            throw new ModelNotLoadedException("Model file 'v1.3.0.bin' not found.");
        }

        return Ok(new { Result = "Success" });
    }
}

public record ClassificationRequest(string Text);
