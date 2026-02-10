
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;

// A simple, immutable Record to represent a chat message in a conversational AI system.
// This structure ensures data integrity and thread-safety when messages are shared across
// multiple processing threads (e.g., for logging, vectorization, or response generation).
public record ChatMessage
{
    // Properties are immutable. They can only be set during initialization via the constructor.
    // This prevents accidental modification after the message is created.
    public string Sender { get; }
    public string Content { get; }
    public DateTime Timestamp { get; }

    // Constructor to initialize the immutable properties.
    // This is the only way to set the state of the record.
    public ChatMessage(string sender, string content)
    {
        // Basic validation to ensure data integrity.
        if (string.IsNullOrWhiteSpace(sender))
            throw new ArgumentException("Sender cannot be null or empty.", nameof(sender));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty.", nameof(content));

        Sender = sender;
        Content = content;
        // Assign the current UTC time to ensure consistency across time zones.
        Timestamp = DateTime.UtcNow;
    }

    // Override ToString() for better debugging and logging output.
    public override string ToString()
    {
        return $"[{Timestamp:HH:mm:ss}] {Sender}: {Content}";
    }
}

// A generic container for a chat session, demonstrating how Records can be composed.
// Generics are allowed in Book 2, enabling flexible type safety for different message payloads.
public record ChatSession<TMessage> where TMessage : ChatMessage
{
    public string SessionId { get; }
    public List<TMessage> Messages { get; } // Using List<T> for collection management.

    public ChatSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

        SessionId = sessionId;
        Messages = new List<TMessage>();
    }

    // Method to add a message to the session.
    // Note: We are not modifying the existing record, but rather the internal list state.
    // In a purely functional approach, we would return a new ChatSession instance.
    // For this intermediate example, we demonstrate state mutation within the session container.
    public void AddMessage(TMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        Messages.Add(message);
    }
}

class Program
{
    static void Main()
    {
        // Problem Context:
        // We are building a logging system for an AI chatbot. 
        // We need to capture user inputs and bot responses immutably to prevent 
        // race conditions when multiple threads access the chat history.
        
        try
        {
            // 1. Create an immutable ChatMessage instance.
            // The 'new' keyword invokes the primary constructor.
            ChatMessage userMessage = new ChatMessage("User", "Hello, can you explain Records?");
            
            // 2. Create a second message for the bot's response.
            ChatMessage botMessage = new ChatMessage("AI Assistant", "Records are immutable data structures.");

            // 3. Create a typed ChatSession to hold these messages.
            // We specify the type as ChatMessage (the base record).
            ChatSession<ChatMessage> session = new ChatSession<ChatMessage>("session-123");

            // 4. Add messages to the session.
            session.AddMessage(userMessage);
            session.AddMessage(botMessage);

            // 5. Display the session contents.
            Console.WriteLine($"Session ID: {session.SessionId}");
            foreach (var msg in session.Messages)
            {
                // Implicitly calls the overridden ToString() method.
                Console.WriteLine(msg.ToString());
            }

            // 6. Demonstrate Immutability:
            // The following line would cause a compilation error because properties are init-only.
            // userMessage.Sender = "Hacker"; // Error: Property or indexer cannot be assigned to -- it is read only
            
            // 7. Demonstrate Pattern Matching (Concept Preview):
            // While advanced pattern matching is covered later, we can check types safely here.
            if (userMessage is ChatMessage cm)
            {
                Console.WriteLine($"Type check passed: {cm.Sender}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
