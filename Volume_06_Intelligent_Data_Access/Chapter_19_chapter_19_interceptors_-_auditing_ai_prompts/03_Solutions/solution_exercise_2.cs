
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

// 1. Data structure for the audit message
public class AuditMessage
{
    public string CommandText { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public DateTime Timestamp { get; set; }
}

// 2. The Channel used for communication
public class AuditChannel
{
    public static readonly Channel<AuditMessage> Channel = Channel.CreateUnbounded<AuditMessage>();
}

// 3. The Interceptor (Producer)
public class AsyncPromptAuditInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        // Fire and forget to the channel, do not await here to avoid blocking the request
        if (command.CommandText.Contains("INSERT INTO \"PromptLogs\""))
        {
            PushToChannel(command);
        }
        return base.ReaderExecuting(command, eventData, result);
    }

    private void PushToChannel(DbCommand command)
    {
        // We must extract data immediately as the command object might be disposed later
        var msg = new AuditMessage
        {
            CommandText = command.CommandText,
            Timestamp = DateTime.UtcNow,
            Parameters = new Dictionary<string, object>()
        };

        foreach (DbParameter p in command.Parameters)
        {
            // Sanitize parameter names for cleaner logs
            msg.Parameters[p.ParameterName] = p.Value;
        }

        // Write to channel without blocking
        AuditChannel.Channel.Writer.TryWrite(msg);
    }
}

// 4. The Background Service (Consumer)
public class AuditBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AuditBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consumer loop
        await foreach (var message in AuditChannel.Channel.Reader.ReadAllAsync(stoppingToken))
        {
            // Create a scope because we are in a singleton service but need scoped DbContext
            using (var scope = _serviceProvider.CreateScope())
            {
                var auditContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

                try
                {
                    // 5. Write to Dedicated Audit Context
                    // In a real scenario, we would map this to an entity. 
                    // Here we simulate the save.
                    Console.WriteLine($"[Background Audit] Processing prompt: {message.Parameters.Values.FirstOrDefault()}");

                    // Simulate DB write
                    // await auditContext.AuditLogs.AddAsync(...);
                    // await auditContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // 6. Error Handling: Log but do not crash the service or affect main transaction
                    Console.WriteLine($"[Background Audit] Failed to write: {ex.Message}");
                }
            }
        }
    }
}

// --- DbContexts ---

public class AppDbContext : DbContext
{
    public DbSet<PromptLog> PromptLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AsyncPromptAuditInterceptor());
    }
}

public class AuditDbContext : DbContext
{
    // Dedicated context for audit storage
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }
}

// --- Startup Configuration (Conceptual) ---
// services.AddHostedService<AuditBackgroundService>();
// services.AddDbContext<AppDbContext>(...);
// services.AddDbContext<AuditDbContext>(...);
