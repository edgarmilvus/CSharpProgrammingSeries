
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

// File: SecureChatHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;

[Authorize] // 1. Enforces authentication on the entire Hub
public class SecureChatHub : Hub
{
    // 2. Static store to map User IDs to Connection IDs.
    // Key: UserId, Value: HashSet of ConnectionIds
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();

    public override async Task OnConnectedAsync()
    {
        // 3. Retrieve the user ID from the claims identity.
        // Context.UserIdentifier is typically populated from the 'sub' or 'name' claim.
        var userId = Context.UserIdentifier;
        
        if (userId != null)
        {
            // Add the current connection ID to the user's set of connections.
            var connections = UserConnections.GetOrAdd(userId, _ => new HashSet<string>());
            lock (connections)
            {
                connections.Add(Context.ConnectionId);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        
        if (userId != null && UserConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(Context.ConnectionId);
                // Clean up the dictionary if the user has no active connections
                if (connections.Count == 0)
                {
                    UserConnections.TryRemove(userId, out _);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // 4. Refactored SendMessage using Context.User
    public async Task SendMessage(string message)
    {
        // We no longer trust the client to send the username; we derive it from the token.
        var user = Context.User?.Identity?.Name ?? "Anonymous";
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    // 5. Private messaging logic
    public async Task SendPrivateMessage(string targetUserId, string message)
    {
        var senderId = Context.UserIdentifier;

        if (senderId == targetUserId)
        {
            throw new HubException("Cannot send a private message to yourself.");
        }

        if (UserConnections.TryGetValue(targetUserId, out var targetConnectionIds))
        {
            // Send to all connections associated with the target user
            // We lock to ensure the set isn't modified while iterating
            List<Task> tasks = new();
            lock (targetConnectionIds)
            {
                foreach (var connId in targetConnectionIds)
                {
                    tasks.Add(Clients.Client(connId).SendAsync("ReceivePrivateMessage", senderId, message));
                }
            }
            await Task.WhenAll(tasks);
        }
        else
        {
            // Optional: Notify sender that the user is offline
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"{targetUserId} is currently offline.");
        }
    }
}

// File: Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add Authentication (Assuming JWT is configured elsewhere)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        // ... standard JWT validation settings
    };

    // 3. Configure JWT events to read token from Query String for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            // Only read from query string for the SignalR endpoint
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/secureChatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSignalR();

var app = builder.Build();

app.UseAuthentication(); // Ensure authentication middleware is active
app.UseAuthorization();

// Map the hub to the secure endpoint
app.MapHub<SecureChatHub>("/secureChatHub");

app.Run();
