
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

// Source File: theory_theoretical_foundations_part8.cs
// Description: Theoretical Foundations
// ==========================================

public class ConversationContext
{
    // Field to store the history (using an array from Chapter 11)
    // We use a private field to hide the complexity.
    private string[] _history = new string[10];
    private int _messageIndex = 0;

    // Property to expose the count of messages
    public int MessageCount 
    { 
        get { return _messageIndex; } 
    }

    // Method to add a message (Behavior)
    public void AddMessage(string message)
    {
        if (_messageIndex < 10)
        {
            _history[_messageIndex] = message;
            _messageIndex++;
        }
    }

    // Method to retrieve the last message
    public string GetLastMessage()
    {
        if (_messageIndex > 0)
        {
            return _history[_messageIndex - 1];
        }
        return "No messages.";
    }
}
