
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

// File: GroupChatHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

public class GroupChatHub : Hub
{
    // Tracks which groups each ConnectionId belongs to.
    // Key: ConnectionId, Value: HashSet of Group Names
    private static readonly ConcurrentDictionary<string, HashSet<string>> ConnectionGroups = new();

    /// <summary>
    /// Adds the calling connection to a specific group.
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        // 1. Add to SignalR's internal group management
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // 2. Update our local state tracking
        var groups = ConnectionGroups.GetOrAdd(Context.ConnectionId, _ => new HashSet<string>());
        lock (groups)
        {
            groups.Add(groupName);
        }

        await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Joined group: {groupName}");
    }

    /// <summary>
    /// Removes the calling connection from a specific group.
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        // 1. Remove from SignalR's internal group management
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        // 2. Update local state
        if (ConnectionGroups.TryGetValue(Context.ConnectionId, out var groups))
        {
            lock (groups)
            {
                groups.Remove(groupName);
                if (groups.Count == 0) ConnectionGroups.TryRemove(Context.ConnectionId, out _);
            }
        }

        await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Left group: {groupName}");
    }

    /// <summary>
    /// Sends a message to a specific group or to everyone.
    /// </summary>
    public async Task SendMessage(string message, string? groupName = null)
    {
        var user = Context.User?.Identity?.Name ?? "Unknown";

        if (!string.IsNullOrEmpty(groupName))
        {
            // Send to specific group
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }
        else
        {
            // Fallback to broadcast
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }

    /// <summary>
    /// Handles cleanup when a connection is lost.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // 1. Retrieve groups for this connection
        if (ConnectionGroups.TryGetValue(Context.ConnectionId, out var groups))
        {
            // 2. Remove connection from all groups it was part of
            foreach (var group in groups)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            }

            // 3. Clean up the dictionary
            ConnectionGroups.TryRemove(Context.ConnectionId, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }
}

// File: Program.cs
// (Standard SignalR setup, no special auth required for this specific exercise logic)
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();
app.MapHub<GroupChatHub>("/groupChatHub");
app.Run();
