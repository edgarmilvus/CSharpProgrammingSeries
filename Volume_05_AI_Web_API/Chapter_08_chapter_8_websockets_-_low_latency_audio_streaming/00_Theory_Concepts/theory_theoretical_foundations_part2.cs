
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

// Conceptual data structures for the stream.
// These are NOT part of the WebSocket protocol itself, but the data we send over it.

// The client sends these as WebSocket frames.
public class AudioMessage
{
    public byte[] AudioData { get; set; } // Binary frame payload
    public bool IsFinalChunk { get; set; } // Metadata flag
}

public class ControlMessage
{
    public string Type { get; set; } // e.g., "START_STREAM", "END_STREAM", "INTERRUPT"
    public Guid ConversationId { get; set; } // Links to Book 4's state management
    public string UserIntent { get; set; } // Optional: Pre-analyzed intent
}

// The server can then process this stream, feeding audio chunks to the model
// and tracking state based on control messages.
// This is the modern C# representation of handling a stream of discriminated union types.
public record WebSocketFrameData(
    FrameType Type, 
    byte[]? AudioPayload, 
    ControlMessage? ControlPayload
);

public enum FrameType { Audio, Control }
