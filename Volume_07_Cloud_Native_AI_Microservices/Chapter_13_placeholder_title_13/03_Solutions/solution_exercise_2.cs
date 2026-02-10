
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

// Program.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// 4. Dependency Injection: Register IMemoryRepository as Singleton
builder.Services.AddSingleton<IMemoryRepository, InMemoryRepository>();

var app = builder.Build();

// 3. API Endpoints
app.MapPost("/memory/{sessionId}", ([FromRoute] Guid sessionId, [FromBody] string context, IMemoryRepository repo) =>
{
    // 5. Concurrency Handling: AddContext is atomic
    var result = repo.AddContext(sessionId, context);
    return result ? Results.Ok() : Results.NotFound();
});

app.MapGet("/memory/{sessionId}", ([FromRoute] Guid sessionId, IMemoryRepository repo) =>
{
    var memory = repo.RetrieveContext(sessionId);
    return memory is not null ? Results.Ok(memory) : Results.NotFound();
});

app.MapDelete("/memory/{sessionId}", ([FromRoute] Guid sessionId, IMemoryRepository repo) =>
{
    repo.ClearSession(sessionId);
    return Results.Ok();
});

app.Run();

// ---------------------------------------------------------
// 1. Data Modeling
public record AgentMemory(
    Guid SessionId,
    string UserId,
    List<string> ContextHistory,
    Dictionary<string, object> Metadata
);

// ---------------------------------------------------------
// 2. In-Memory Repository Interface
public interface IMemoryRepository
{
    bool AddContext(Guid sessionId, string context);
    AgentMemory? RetrieveContext(Guid sessionId);
    void ClearSession(Guid sessionId);
}

// ---------------------------------------------------------
// 2. In-Memory Repository Implementation
public class InMemoryRepository : IMemoryRepository
{
    // Thread-safe storage
    private readonly ConcurrentDictionary<Guid, AgentMemory> _store = new();

    public bool AddContext(Guid sessionId, string context)
    {
        // Atomic operation using ConcurrentDictionary's GetOrAdd
        // We update the list inside a lock to ensure thread safety of the list itself
        // or use a concurrent list. Here we use locking for simplicity on the List<string>.
        var memory = _store.GetOrAdd(sessionId, id => new AgentMemory(
            SessionId: id,
            UserId: "Anonymous", // Simplified for demo
            ContextHistory: new List<string>(),
            Metadata: new Dictionary<string, object>()
        ));

        // Locking ensures the List<string> modification is thread-safe
        lock (memory.ContextHistory)
        {
            memory.ContextHistory.Add(context);
        }

        return true;
    }

    public AgentMemory? RetrieveContext(Guid sessionId)
    {
        _store.TryGetValue(sessionId, out var memory);
        return memory;
    }

    public void ClearSession(Guid sessionId)
    {
        _store.TryRemove(sessionId, out _);
    }
}
