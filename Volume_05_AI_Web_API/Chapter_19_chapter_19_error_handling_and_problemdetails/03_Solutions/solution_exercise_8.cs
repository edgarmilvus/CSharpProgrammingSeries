
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

// Source File: solution_exercise_8.cs
// Description: Solution for Exercise 8
// ==========================================

// 1. Program.cs Configuration (Structured Logging)
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog to write JSON to the console
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog as the logging provider

builder.Services.AddExceptionHandler<AiServiceExceptionHandler>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();

app.Run();

// 2. Updated AiServiceExceptionHandler with Structured Logging
public class AiServiceExceptionHandler : IExceptionHandler
{
    private readonly ILogger<AiServiceExceptionHandler> _logger;

    public AiServiceExceptionHandler(ILogger<AiServiceExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(exception); // Reuse logic from Ex 1

        // Extract TraceId (Correlation ID)
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Structured Logging
        // We use a scope or properties to attach context to the log entry
        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("ProblemType", problemDetails.Type))
        using (LogContext.PushProperty("ProblemTitle", problemDetails.Title))
        using (LogContext.PushProperty("RequestPath", httpContext.Request.Path))
        {
            _logger.LogError(exception, "Exception handled and converted to ProblemDetails");
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
    
    // Helper method to create ProblemDetails (omitted for brevity, same as Ex 1)
    private ProblemDetails CreateProblemDetails(Exception ex) { /* ... logic ... */ return new ProblemDetails(); }
}
