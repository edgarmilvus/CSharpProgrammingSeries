
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

// --- Approach 1: Using DbCommandInterceptor (The original exercise requirement) ---

public class PerformanceAuditInterceptor : DbCommandInterceptor
{
    // We use a dictionary to store start times per command instance (keyed by hash or instance)
    // However, since Interceptors are singleton, we can use the command object itself as a key if we are careful,
    // but a simpler approach for this demo is to store the start time in a property bag if available, 
    // or simply rely on the fact that ReaderExecuting runs immediately before the Reader is consumed.
    
    // Actually, the standard pattern is to start a stopwatch in 'ReaderExecuting' and stop it in 'ReaderFinished' (or ReaderFailed).
    
    private readonly Stopwatch _stopwatch = new Stopwatch();

    public override DbDataReader ReaderExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        DbDataReader result)
    {
        // Start timing
        command.CommandTimeout = 30; // Ensure timeout
        
        // Store start time in the command's parameter collection or a static dictionary
        // For simplicity, we'll use a static concurrent dictionary in a real app, 
        // but here we will rely on the 'ReaderExecuted' to calculate total time.
        return base.ReaderExecuting(command, eventData, result);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result)
    {
        // Calculate duration
        // eventData.Duration is available in CommandExecutedEventData in EF Core 5+
        var elapsedMs = eventData.Duration.TotalMilliseconds;

        AnalyzeAndLog(command, elapsedMs);
        
        return base.ReaderExecuted(command, eventData, result);
    }

    // Helper to calculate complexity and log
    private void AnalyzeAndLog(DbCommand command, double elapsedMs)
    {
        // 2. Metric Calculation
        bool isSlow = elapsedMs > 200; // 200ms threshold

        // 3. SQL Analysis (Simple string parsing)
        string sql = command.CommandText.ToUpper();
        int joinCount = (sql.Length - sql.Replace("JOIN", "").Length) / 4;
        int whereCount = (sql.Length - sql.Replace("WHERE", "").Length) / 5;
        int complexityScore = joinCount + whereCount;

        // 4. Conditional Logging
        bool detailedLogging = true; // Should come from config

        if (isSlow || detailedLogging)
        {
            var logEntry = new 
            {
                Command = command.CommandText.Substring(0, Math.Min(command.CommandText.Length, 50)) + "...",
                ExecutionTimeMs = elapsedMs,
                ComplexityScore = complexityScore,
                IsSlow = isSlow
            };
            
            // Persist to DB or Log
            Console.WriteLine($"[PERF LOG]: {JsonSerializer.Serialize(logEntry)}");
        }
    }
}

// --- Approach 2: Using DiagnosticListener (Interactive Challenge) ---

public class EfCoreDiagnosticObserver : IObserver<KeyValuePair<string, object>>
{
    public void OnCompleted() { }
    public void OnError(Exception error) { }

    public void OnNext(KeyValuePair<string, object> kvp)
    {
        // Listen to specific EF Core events
        if (kvp.Key == "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")
        {
            // Extract data from dynamic event payload
            dynamic data = kvp.Value;
            var command = data.Command as DbCommand;
            var timestamp = Stopwatch.GetTimestamp();
            
            // We need to store this timestamp somewhere (e.g., AsyncLocal) to match with CommandExecuted
        }
        else if (kvp.Key == "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")
        {
            dynamic data = kvp.Value;
            var command = data.Command as DbCommand;
            var duration = data.Duration; // TimeSpan

            // Perform same analysis as above
            if (duration.TotalMilliseconds > 200)
            {
                Console.WriteLine($"[DIAGNOSTIC] Slow Query Detected: {duration.TotalMilliseconds}ms");
            }
        }
    }
}

// --- Comparison Paragraph ---
/*
Comparison: The `IDbCommandInterceptor` approach is tightly coupled to the EF Core `DbContext` configuration. 
It is explicit and easy to implement for simple scenarios. However, `DiagnosticListener` is a global .NET mechanism. 
It allows you to subscribe to EF Core events without modifying the `DbContext` options or adding interceptors explicitly 
in every context. This makes `DiagnosticListener` superior for cross-cutting concerns (like APM tools) and libraries, 
as it is decoupled and has a lower overhead for the application code itself, though it requires more complex event parsing.
*/
