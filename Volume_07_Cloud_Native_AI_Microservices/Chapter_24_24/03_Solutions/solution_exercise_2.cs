
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

// Models.cs
public record Message(string Role, string Content);

public record ConversationSession
{
    public Guid SessionId { get; init; }
    public string UserId { get; init; }
    public List<Message> Messages { get; init; } = new();
    public DateTimeOffset Timestamp { get; init; }
}

public record ChatRequest(Guid SessionId, string UserMessage);

// IStateStore.cs
public interface IStateStore
{
    Task<ConversationSession?> GetSessionAsync(Guid sessionId);
    Task UpdateSessionAsync(ConversationSession session);
    Task CreateSessionAsync(ConversationSession session);
}

// RedisStateStore.cs
using StackExchange.Redis;
using System.Text.Json;

public class RedisStateStore : IStateStore
{
    private readonly IConnectionMultiplexer _redis;

    public RedisStateStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<ConversationSession?> GetSessionAsync(Guid sessionId)
    {
        var db = _redis.GetDatabase();
        var data = await db.StringGetAsync($"session:{sessionId}");
        if (data.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<ConversationSession>(data!);
    }

    public async Task UpdateSessionAsync(ConversationSession session)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(session);
        await db.StringSetAsync($"session:{session.SessionId}", json);
    }

    public async Task CreateSessionAsync(ConversationSession session)
    {
        await UpdateSessionAsync(session);
    }
}

// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 1. Dependency Injection & Redis Configuration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<IStateStore, RedisStateStore>();

var app = builder.Build();

// 2. API Endpoint
app.MapPost("/chat", async (ChatRequest request, IStateStore stateStore) =>
{
    // Retrieve session
    var session = await stateStore.GetSessionAsync(request.SessionId);
    
    // If session doesn't exist, create a new one
    if (session == null)
    {
        session = new ConversationSession
        {
            SessionId = request.SessionId,
            UserId = "anonymous", // In real app, extract from auth
            Messages = new List<Message>(),
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    // Append user message
    session = session with 
    { 
        Messages = new List<Message>(session.Messages) 
        { 
            new Message("User", request.UserMessage) 
        },
        Timestamp = DateTimeOffset.UtcNow 
    };

    // Mock AI Response
    var aiResponse = $"Echo: {request.UserMessage}";
    session = session with
    {
        Messages = new List<Message>(session.Messages)
        {
            new Message("Assistant", aiResponse)
        }
    };

    // Save back to Redis
    await stateStore.UpdateSessionAsync(session);

    return Results.Ok(session.Messages);
});

app.Run();
