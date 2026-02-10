
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

using System.Buffers;
using System.Net.WebSockets;
using System.Text;

namespace LowLatencyAudioStreaming
{
    public class AudioWebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public AudioWebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Check if the request is a WebSocket upgrade request
            if (context.WebSockets.IsWebSocketRequest && 
                context.Request.Path.StartsWithSegments("/audio-stream"))
            {
                // 2. Accept the WebSocket connection
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                
                // 3. Start the bidirectional audio processing loop
                await HandleAudioStream(webSocket);
            }
            else
            {
                // 4. Pass non-WebSocket requests down the pipeline
                await _next(context);
            }
        }

        private async Task HandleAudioStream(WebSocket webSocket)
        {
            // Buffer for incoming audio chunks (approx 4KB, typical for Opus/WebRTC frames)
            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            
            try
            {
                // 5. Configure cancellation token for graceful shutdown
                using var cts = new CancellationTokenSource();
                
                // 6. Start a background task to simulate AI processing (e.g., voice recognition)
                var processingTask = Task.Run(() => SimulateAIProcessing(cts.Token));

                // 7. Continuous receive loop
                while (webSocket.State == WebSocketState.Open)
                {
                    // 8. Asynchronously receive audio data from the client
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), 
                        cts.Token);

                    // 9. Handle connection closure
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure, 
                            "Closing", 
                            CancellationToken.None);
                        break;
                    }

                    // 10. Process the audio chunk immediately (Echo for demo, real-world: send to AI model)
                    // In a real scenario, this buffer would be pushed to a thread-safe queue 
                    // consumed by the AI processing engine.
                    if (result.Count > 0)
                    {
                        // 11. Send acknowledgment back to client (Low latency feedback)
                        var responseMessage = Encoding.UTF8.GetBytes($"ACK: {result.Count} bytes");
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(responseMessage),
                            WebSocketMessageType.Text,
                            true,
                            cts.Token);
                    }
                }
                
                // 12. Signal processing loop to stop
                cts.Cancel();
            }
            finally
            {
                // 13. Return buffer to the pool to prevent GC pressure
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        // 14. Mock AI processing method
        private async Task SimulateAIProcessing(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 15. Simulate processing time (e.g., neural network inference)
                await Task.Delay(50, token); 
                // In production: Check a ConcurrentQueue for new audio buffers
            }
        }
    }
}
