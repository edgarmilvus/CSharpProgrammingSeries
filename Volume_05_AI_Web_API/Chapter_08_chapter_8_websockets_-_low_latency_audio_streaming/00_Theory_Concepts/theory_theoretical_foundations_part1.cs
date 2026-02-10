
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

// Conceptual Client-Side Handshake Request
// This is not server code, but an illustration of the initial HTTP request headers
// that a browser or client library would generate to initiate a WebSocket connection.

GET /api/audio/chat HTTP/1.1
Host: ai-voice-assistant.com
Upgrade: websocket          // Tells the server: "I want to switch protocols"
Connection: Upgrade          // Informs intermediaries to not close this connection
Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ== // A randomly generated key for validation
Sec-WebSocket-Version: 13   // The WebSocket protocol version
