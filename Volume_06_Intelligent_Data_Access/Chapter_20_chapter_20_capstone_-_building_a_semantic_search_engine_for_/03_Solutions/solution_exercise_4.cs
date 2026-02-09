
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// 1. Entity Definition
public class MemoryEntry
{
    public Guid Id { get; set; }
    public string QueryHash { get; set; } = string.Empty;
    public string ContextJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UsageCount { get; set; }
    public DateTime? ExpiresAt { get; set; } // For TTL
}

public class CacheDbContext : DbContext
{
    public DbSet<MemoryEntry> MemoryEntries { get; set; }

    public CacheDbContext(DbContextOptions<CacheDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemoryEntry>()
            .HasIndex(e => e.QueryHash)
            .IsUnique();
        
        modelBuilder.Entity<MemoryEntry>()
            .HasIndex(e => e.ExpiresAt); // Index for cleanup
    }
}

// 2. RagMemoryStore Implementation
public class RagMemoryStore
{
    private readonly CacheDbContext _context;
    private readonly ILogger<RagMemoryStore> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1); // Simple lock for concurrency
    private readonly int _maxCapacity;
    private readonly int _ttlSeconds;

    public RagMemoryStore(CacheDbContext context, ILogger<RagMemoryStore> logger, int maxCapacity = 1000, int ttlSeconds = 3600)
    {
        _context = context;
        _logger = logger;
        _maxCapacity = maxCapacity;
        _ttlSeconds = ttlSeconds;
    }

    public async Task<IEnumerable<DocumentChunk>> GetOrSetAsync(
        string query, 
        Func<Task<IEnumerable<DocumentChunk>>> retrieveContextFunc,
        CancellationToken ct = default)
    {
        var hash = ComputeSha256Hash(query);
        
        await _semaphore.WaitAsync(ct);
        try
        {
            // Check for existing entry
            var entry = await _context.MemoryEntries
                .FirstOrDefaultAsync(e => e.QueryHash == hash, ct);

            // Check TTL
            if (entry != null && entry.ExpiresAt > DateTime.UtcNow)
            {
                // Cache Hit
                entry.UsageCount++;
                entry.CreatedAt = DateTime.UtcNow; // Update last accessed
                await _context.SaveChangesAsync(ct);
                
                _logger.LogInformation("Cache hit for query hash: {Hash}", hash);
                return JsonSerializer.Deserialize<List<DocumentChunk>>(entry.ContextJson) ?? new List<DocumentChunk>();
            }
            
            // Cache Miss or Expired
            if (entry != null && entry.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogInformation("Cache expired for query hash: {Hash}", hash);
            }

            // Execute expensive operation
            var context = await retrieveContextFunc();
            var json = JsonSerializer.Serialize(context);

            if (entry == null)
            {
                // New Entry
                entry = new MemoryEntry
                {
                    Id = Guid.NewGuid(),
                    QueryHash = hash,
                    ContextJson = json,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(_ttlSeconds),
                    UsageCount = 1
                };
                _context.MemoryEntries.Add(entry);
            }
            else
            {
                // Update Expired Entry
                entry.ContextJson = json;
                entry.CreatedAt = DateTime.UtcNow;
                entry.ExpiresAt = DateTime.UtcNow.AddSeconds(_ttlSeconds);
                entry.UsageCount++;
            }

            // Check Eviction Policy before saving if adding new entry
            if (_context.Entry(entry).State == EntityState.Added)
            {
                await EnforceCapacityAsync(ct);
            }

            await _context.SaveChangesAsync(ct);
            return context;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task EnforceCapacityAsync(CancellationToken ct)
    {
        var count = await _context.MemoryEntries.CountAsync(ct);
        if (count >= _maxCapacity)
        {
            _logger.LogWarning("Cache capacity reached. Initiating LFU eviction.");

            // LFU Eviction: Delete 50 entries with lowest UsageCount, tie-break with oldest CreatedAt
            var entriesToRemove = await _context.MemoryEntries
                .OrderBy(e => e.UsageCount)
                .ThenBy(e => e.CreatedAt)
                .Take(50)
                .ToListAsync(ct);

            _context.MemoryEntries.RemoveRange(entriesToRemove);
            // Note: SaveChangesAsync is called by the caller
        }
    }

    // Interactive Challenge: Background Cleanup for TTL
    public async Task CleanupExpiredEntriesAsync(CancellationToken ct)
    {
        var expired = await _context.MemoryEntries
            .Where(e => e.ExpiresAt != null && e.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(ct);

        if (expired.Any())
        {
            _context.MemoryEntries.RemoveRange(expired);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Cleaned up {Count} expired cache entries", expired.Count);
        }
    }

    private static string ComputeSha256Hash(string input)
    {
        using SHA256 sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder builder = new();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}

// 3. Background Service for Cleanup
public class CacheCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheCleanupService> _logger;

    public CacheCleanupService(IServiceProvider serviceProvider, ILogger<CacheCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a scope because DbContext is scoped
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cacheStore = scope.ServiceProvider.GetRequiredService<RagMemoryStore>();
                    await cacheStore.CleanupExpiredEntriesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache cleanup");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
