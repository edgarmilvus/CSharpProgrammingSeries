
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

using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

// --- EF Core Persistence Models ---

public class SessionData
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string ContextJson { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class MemoryDbContext : DbContext
{
    public DbSet<SessionData> Sessions { get; set; }
    
    public MemoryDbContext(DbContextOptions<MemoryDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure we can query by TenantId
        modelBuilder.Entity<SessionData>().HasIndex(s => s.TenantId);
    }
}

// --- Tenant Memory Service ---

public class TenantMemoryService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly MemoryDbContext _dbContext;
    private readonly string? _currentTenantId;

    public TenantMemoryService(IConnectionMultiplexer redis, MemoryDbContext dbContext, string? currentTenantId = null)
    {
        _redis = redis;
        _dbContext = dbContext;
        _currentTenantId = currentTenantId;
    }

    // Helper to generate tenant-specific keys
    private string GetKey(string sessionId) => $"{_currentTenantId}:session:{sessionId}";

    public async Task StoreContextAsync(string sessionId, string context)
    {
        if (string.IsNullOrEmpty(_currentTenantId)) throw new InvalidOperationException("Tenant not set.");

        var db = _redis.GetDatabase();
        var key = GetKey(sessionId);
        
        // Store in Redis with expiration (e.g., 1 hour)
        await db.StringSetAsync(key, context, TimeSpan.FromHours(1));
    }

    public async Task<string?> RetrieveContextAsync(string sessionId)
    {
        if (string.IsNullOrEmpty(_currentTenantId)) return null;

        var db = _redis.GetDatabase();
        var key = GetKey(sessionId);
        
        return await db.StringGetAsync(key);
    }

    public async Task SyncMemoryAsync(string sessionId)
    {
        if (string.IsNullOrEmpty(_currentTenantId)) return;

        // Retrieve from Redis
        var context = await RetrieveContextAsync(sessionId);
        if (context == null) return;

        // Upsert to EF Core (Persistence)
        var existing = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.TenantId == _currentTenantId && s.SessionId == sessionId);

        if (existing != null)
        {
            existing.ContextJson = context;
            existing.LastUpdated = DateTime.UtcNow;
        }
        else
        {
            _dbContext.Sessions.Add(new SessionData
            {
                Id = Guid.NewGuid(),
                TenantId = _currentTenantId,
                SessionId = sessionId,
                ContextJson = context,
                LastUpdated = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}

// --- Memory Retriever for RAG ---

public class MemoryRetriever
{
    private readonly TenantMemoryService _memoryService;

    public MemoryRetriever(TenantMemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public async Task<string> RetrieveRelevantContextAsync(string sessionId)
    {
        // 1. Check Redis (Fast path)
        var redisContext = await _memoryService.RetrieveContextAsync(sessionId);
        
        if (!string.IsNullOrEmpty(redisContext))
        {
            return redisContext;
        }

        // 2. Fallback to EF Core (Slow path)
        // Note: In a real scenario, we'd inject the DbContext directly here or via service.
        // For this example, we assume the MemoryService handles the sync logic or we access DB separately.
        // Let's simulate retrieving from DB if Redis misses.
        
        // Assuming we have access to the DbContext here (simplified for the exercise)
        // var dbContext = ... 
        // var dbSession = await dbContext.Sessions.FirstOrDefaultAsync(...)
        
        return "Fallback context from DB (omitted for brevity)";
    }
}

// --- Interactive Challenge: Cross-Tenant Access ---

public class AdminMemoryService : TenantMemoryService
{
    public AdminMemoryService(IConnectionMultiplexer redis, MemoryDbContext dbContext, string currentTenantId) 
        : base(redis, dbContext, currentTenantId) { }

    public async Task<string?> RetrieveContextWithPermissionAsync(string targetSessionId, string targetTenantId, string adminToken)
    {
        // 1. Validate Admin Token (Mock validation)
        if (!await ValidateAdminTokenAsync(adminToken)) 
            return null;

        // 2. Temporarily switch context to target tenant to use the base logic or construct key manually
        // Construct key manually to avoid changing the service state
        var key = $"{targetTenantId}:session:{targetSessionId}";
        
        var db = _redis.GetDatabase();
        return await db.StringGetAsync(key);
    }

    private Task<bool> ValidateAdminTokenAsync(string token)
    {
        // In reality, check against Identity Provider or DB
        return Task.FromResult(token == "SuperSecretAdminToken");
    }
}
