
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ConversationContext
{
    public string SessionId { get; set; }
    public List<string> Messages { get; set; }

    public ConversationContext(string sessionId)
    {
        SessionId = sessionId;
        Messages = new List<string>();
    }

    // Private constructor for deserialization
    private ConversationContext() { }

    public void SaveToBinary(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs, Encoding.UTF8))
        {
            // 1. Write SessionId
            writer.Write(SessionId);

            // 2. Write Message Count (Critical for reading lists back)
            writer.Write(Messages.Count);

            // 3. Write each message
            foreach (var msg in Messages)
            {
                writer.Write(msg);
            }
        }
        Console.WriteLine($"Binary context saved to {filePath}");
    }

    public static ConversationContext LoadFromBinary(string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException();

        var context = new ConversationContext();
        
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs, Encoding.UTF8))
        {
            // 1. Read SessionId
            context.SessionId = reader.ReadString();

            // 2. Read Message Count
            int count = reader.ReadInt32();
            context.Messages = new List<string>(count);

            // 3. Read messages
            for (int i = 0; i < count; i++)
            {
                context.Messages.Add(reader.ReadString());
            }
        }
        return context;
    }
}

// Usage Example
public class BinaryChallenge
{
    public static void Run()
    {
        var ctx = new ConversationContext("conv-101");
        ctx.Messages.Add("Hello, AI.");
        ctx.Messages.Add("How are you processing tensors today?");

        ctx.SaveToBinary("context.bin");

        var loadedCtx = ConversationContext.LoadFromBinary("context.bin");
        Console.WriteLine($"Session: {loadedCtx.SessionId}");
        foreach (var msg in loadedCtx.Messages)
        {
            Console.WriteLine($" - {msg}");
        }
    }
}
