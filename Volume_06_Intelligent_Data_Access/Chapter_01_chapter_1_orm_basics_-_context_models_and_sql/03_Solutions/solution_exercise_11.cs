
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

// Source File: solution_exercise_11.cs
// Description: Solution for Exercise 11
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Domain Events
public abstract record DomainEvent(Guid AggregateId, DateTime OccurredOn);

public record DocumentCreated(Guid Id, string Title, string Content) : DomainEvent(Id, DateTime.UtcNow);
public record DocumentUpdated(Guid Id, string NewContent) : DomainEvent(Id, DateTime.UtcNow);
public record VectorEmbeddingCalculated(Guid Id, float[] Vector) : DomainEvent(Id, DateTime.UtcNow);

// 2. Aggregate Root (Document)
public class DocumentAggregate
{
    public Guid Id { get; private set; }
    public string Content { get; private set; }
    public float[] Vector { get; private set; }
    public int Version { get; private set; }
    
    private readonly List<DomainEvent> _uncommittedEvents = new();

    public DocumentAggregate() { }

    // Replay logic
    public void Apply(DomainEvent e)
    {
        switch (e)
        {
            case DocumentCreated c:
                Id = c.Id;
                Content = c.Content;
                Version = 1;
                break;
            case DocumentUpdated u:
                Content = u.NewContent;
                Version++;
                break;
            case VectorEmbeddingCalculated v:
                Vector = v.Vector;
                break;
        }
    }

    // Business Logic
    public void UpdateContent(string newContent)
    {
        if (Content != newContent)
        {
            var evt = new DocumentUpdated(Id, newContent);
            _uncommittedEvents.Add(evt);
            Apply(evt);
        }
    }

    public IEnumerable<DomainEvent> GetUncommittedEvents() => _uncommittedEvents;
    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();
}

// 3. Event Store (Simplified)
public class EventStore
{
    public async Task AppendAsync(IEnumerable<DomainEvent> events)
    {
        // Store events in a relational table (JSON column)
        // This is the "Source of Truth"
    }

    public async Task<DocumentAggregate> LoadAsync(Guid id)
    {
        // 4. Snapshot Mechanism
        // Check if a recent snapshot exists
        var snapshot = await LoadSnapshotAsync(id);
        var events = await GetEventsSinceSnapshotAsync(id, snapshot?.Version ?? 0);

        var doc = new DocumentAggregate();
        if (snapshot != null) doc.Apply(snapshot);
        foreach (var e in events) doc.Apply(e);
        
        return doc;
    }

    private async Task<DocumentAggregate> LoadSnapshotAsync(Guid id) => null; // Mock
    private async Task<List<DomainEvent>> GetEventsSinceSnapshotAsync(Guid id, int version) => new(); // Mock
}

// 5. Event Handler (Async Vector Regeneration)
public class VectorRegenerationHandler
{
    private readonly IEmbeddingService _embeddingService;

    public async Task Handle(DocumentUpdated evt)
    {
        // Calculate new vector
        var vector = await _embeddingService.GenerateAsync(evt.NewContent);
        
        // Publish VectorEmbeddingCalculated event
        var vectorEvt = new VectorEmbeddingCalculated(evt.Id, vector);
        
        // Update the Read Model (Vector DB)
        // This is eventually consistent
        await UpdateVectorReadModel(vectorEvt);
    }

    private async Task UpdateVectorReadModel(VectorEmbeddingCalculated evt)
    {
        // Update the vector database (Milvus, Pinecone, etc.)
    }
}

// Mocks
public interface IEmbeddingService { Task<float[]> GenerateAsync(string text); }
