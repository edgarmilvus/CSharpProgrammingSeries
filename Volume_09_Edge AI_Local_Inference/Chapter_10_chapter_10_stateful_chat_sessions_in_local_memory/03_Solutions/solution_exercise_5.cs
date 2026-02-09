
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class SessionManager
{
    private readonly JsonSerializerOptions _options;

    public SessionManager()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    // Requirement 4: Security/Obfuscation
    private string ObfuscateContent(string content)
    {
        // Simple Base64 encoding for demonstration.
        // Note: Base64 is encoding, not encryption. 
        // For real PII, consider AES encryption with a user-provided key or OS KeyVault.
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
    }

    private string DeobfuscateContent(string obfuscatedContent)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(obfuscatedContent));
        }
        catch
        {
            return obfuscatedContent; // Fallback if not obfuscated
        }
    }

    public void SaveSession(string filePath, ConcurrentConversationBuffer buffer)
    {
        // Snapshot the queue
        var messages = buffer.GetContext(int.MaxValue, _ => 0).ToList(); // Hack to get all messages

        // For saving, we might want to obfuscate User/Assistant content if PII is a concern.
        // We create a DTO (Data Transfer Object) for serialization to handle this cleanly.
        var serializableMessages = messages.Select(m => new 
        {
            Role = m.Role,
            // Obfuscate content for security
            Content = ObfuscateContent(m.Content), 
            Timestamp = m.Timestamp
        }).ToList();

        string json = JsonSerializer.Serialize(serializableMessages, _options);
        File.WriteAllText(filePath, json);
        Console.WriteLine($"Session saved to {filePath}");
    }

    public ConcurrentConversationBuffer LoadSession(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("No existing session found.");
            return new ConcurrentConversationBuffer();
        }

        string json = File.ReadAllText(filePath);
        
        // We need to deserialize into a structure that matches our buffer.
        // Since we used anonymous types for saving, we define a strict DTO here for loading.
        var deserializedMessages = JsonSerializer.Deserialize<ChatMessageDto[]>(json, _options);

        var buffer = new ConcurrentConversationBuffer();
        foreach (var dto in deserializedMessages)
        {
            // Deobfuscate content
            string content = DeobfuscateContent(dto.Content);
            // Re-enqueue
            // Note: ConcurrentQueue doesn't have a direct Add range, so we iterate.
            // We use the public AddMessage method to maintain timestamp consistency if desired,
            // or reconstruct the record directly if we expose the queue.
            // For this solution, we will reconstruct the record and Enqueue directly to the internal structure
            // or expose a method. Let's assume we add a method to ConcurrentConversationBuffer to load bulk data.
            
            // Simulating the AddMessage logic but without generating a new timestamp if we want to preserve original
            // For simplicity, we'll just use AddMessage here, accepting the timestamp update or 
            // modify AddMessage to accept an optional timestamp.
            buffer.AddMessage(dto.Role, content); 
        }

        Console.WriteLine($"Session loaded from {filePath}. {buffer.Count} messages.");
        return buffer;
    }

    // DTO for Deserialization
    private class ChatMessageDto
    {
        public ChatMessageRole Role { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

// Integration into ChatSession.exe (Exercise 3 extension)
public class PersistentChatSession
{
    private readonly SessionManager _sessionManager = new();
    private const string DefaultSessionFile = "session.json";

    // Usage example within the application loop:
    public void HandleCommand(string input)
    {
        if (input.StartsWith("/save"))
        {
            var parts = input.Split(' ');
            string filename = parts.Length > 1 ? parts[1] : DefaultSessionFile;
            // _sessionManager.SaveSession(filename, _buffer); 
        }
        else if (input.StartsWith("/load"))
        {
            // _buffer = _sessionManager.LoadSession(DefaultSessionFile);
        }
    }
}
