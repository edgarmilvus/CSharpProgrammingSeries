
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

# Source File: solution_exercise_12.cs
# Description: Solution for Exercise 12
# ==========================================

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

// 1. SignalR Hub
public class VectorSearchHub : Hub
{
    public async Task SubscribeToSearch(string queryId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, queryId);
    }

    public async Task UnsubscribeFromSearch(string queryId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, queryId);
    }
}

// 2. Change Tracker Integration
public class SignalRDbContext : DbContext
{
    private readonly IHubContext<VectorSearchHub> _hubContext;

    public SignalRDbContext(DbContextOptions options, IHubContext<VectorSearchHub> hubContext) 
        : base(options)
    {
        _hubContext = hubContext;
    }

    public DbSet<Document> Documents { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Detect changes before saving
        var changedDocs = ChangeTracker.Entries<Document>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // 3. Broadcast updates
        foreach (var doc in changedDocs)
        {
            // Notify clients that this specific document changed
            await _hubContext.Clients.All.SendAsync("DocumentUpdated", doc.Id);

            // If we have a specific search group for this doc's content, notify them
            // (In reality, you'd need a mapping of active queries to document IDs)
            // await _hubContext.Clients.Group("query-id").SendAsync("SearchResultsChanged");
        }

        return result;
    }
}

// 4. Real-time Search Service
public class RealTimeSearchService
{
    private readonly IHubContext<VectorSearchHub> _hubContext;
    private readonly AppDbContext _context;

    public async Task PerformSearch(string queryId, float[] vector)
    {
        // Initial Search
        var results = await _context.Documents
            .Select(d => new { d.Id, Score = CalculateSim(d.Vector, vector) })
            .Where(x => x.Score > 0.7)
            .ToListAsync();

        // Send initial results
        await _hubContext.Clients.Group(queryId).SendAsync("SearchResults", results);

        // 5. Backpressure Mechanism
        // We don't continuously poll. We wait for the DbContext to trigger updates via SaveChangesAsync (above).
        // However, if we need to monitor the vector DB for external updates:
        
        // Start a background loop (simplified)
        // while (true) { await Task.Delay(5000); /* Re-check and Notify */ }
    }

    private double CalculateSim(float[] a, float[] b) => 0.9; // Mock
}
