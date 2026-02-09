
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SseStreamSimulator
{
    // Simulates a real-time stock price feed using SSE concepts.
    // This mimics the behavior of an LLM streaming endpoint but for financial data.
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting SSE Stock Stream Simulator...");
            Console.WriteLine("Listening for connections on http://localhost:8080/");
            Console.WriteLine("Press Ctrl+C to stop.");

            // Create a listener to simulate an ASP.NET Core server
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();

            // Continuous loop to accept client connections
            while (true)
            {
                // Wait for a client (browser) to connect
                HttpListenerContext context = await listener.GetContextAsync();
                
                // Handle the request asynchronously without blocking the listener
                _ = Task.Run(() => HandleRequest(context));
            }
        }

        // Handles the logic for a single client connection
        static async Task HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Check if the client is requesting the HTML interface
            if (request.Url.AbsolutePath == "/")
            {
                ServeHtml(response);
                return;
            }

            // Check if the client is requesting the SSE stream
            if (request.Url.AbsolutePath == "/stream")
            {
                await ServeSseStream(response);
                return;
            }

            // Handle 404 for unknown paths
            byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
            response.StatusCode = 404;
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }

        // Serves the frontend HTML page
        static void ServeHtml(HttpListenerResponse response)
        {
            string html = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>SSE Stock Stream</title>
                    <style>
                        body { font-family: 'Segoe UI', sans-serif; background: #1e1e1e; color: #d4d4d4; padding: 20px; }
                        #stock-display { font-size: 2em; margin-top: 20px; }
                        .price-up { color: #4caf50; }
                        .price-down { color: #f44336; }
                        #log { margin-top: 20px; border-top: 1px solid #444; padding-top: 10px; font-family: monospace; }
                    </style>
                </head>
                <body>
                    <h1>Real-Time Stock Ticker (SSE)</h1>
                    <div id='stock-display'>Waiting for stream...</div>
                    <div id='log'></div>
                    
                    <script>
                        const display = document.getElementById('stock-display');
                        const log = document.getElementById('log');
                        
                        // Native EventSource API to consume SSE
                        const eventSource = new EventSource('/stream');

                        // Listen for the custom 'price-update' event
                        eventSource.addEventListener('price-update', (e) => {
                            const data = JSON.parse(e.data);
                            const priceStr = `$${data.price.toFixed(2)}`;
                            
                            // Update UI based on price change
                            display.textContent = priceStr;
                            display.className = data.change > 0 ? 'price-up' : 'price-down';
                            
                            // Log the event
                            const entry = document.createElement('div');
                            entry.textContent = `[${new Date().toLocaleTimeString()}] Received: ${priceStr} (Delta: ${data.change > 0 ? '+' : ''}${data.change})`;
                            log.prepend(entry);
                        });

                        // Handle connection errors
                        eventSource.onerror = () => {
                            display.textContent = 'Connection lost. Refreshing...';
                            setTimeout(() => location.reload(), 2000);
                        };
                    </script>
                </body>
                </html>";

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        // Streams data using Server-Sent Events protocol
        static async Task ServeSseStream(HttpListenerResponse response)
        {
            // CRITICAL: SSE requires specific headers
            response.ContentType = "text/event-stream";
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");
            
            // Simulate stock data generation
            double currentPrice = 150.00;
            Random random = new Random();
            
            // We use a cancellation token to stop the stream if the client disconnects
            using (var cts = new CancellationTokenSource())
            {
                // Simulate client disconnect detection (simplified)
                _ = Task.Run(() => {
                    try { while (true) Thread.Sleep(1000); } 
                    catch { cts.Cancel(); }
                });

                try
                {
                    // Stream loop: Mimics IAsyncEnumerable<T> behavior in ASP.NET Core
                    for (int i = 0; i < 20; i++) // Stream 20 updates then close
                    {
                        // Check if client is still connected
                        if (!response.OutputStream.CanWrite) break;

                        // Simulate generation delay (like an LLM thinking)
                        await Task.Delay(1000, cts.Token);

                        // Generate data
                        double change = (random.NextDouble() - 0.5) * 2.0; // Random between -1 and 1
                        currentPrice += change;
                        
                        // Format SSE payload
                        // Format: event: {name}\ndata: {json}\n\n
                        string eventName = "price-update";
                        string jsonData = $@"{{""price"":{currentPrice:F2}, ""change"":{change:F2}}}";
                        
                        StringBuilder sseBuilder = new StringBuilder();
                        sseBuilder.AppendLine($"event: {eventName}");
                        sseBuilder.AppendLine($"data: {jsonData}");
                        sseBuilder.AppendLine(); // Double newline is required to terminate an event

                        // Convert to bytes
                        byte[] buffer = Encoding.UTF8.GetBytes(sseBuilder.ToString());
                        
                        // Write to the output stream immediately
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        
                        // Flush ensures data is sent immediately rather than buffered
                        await response.OutputStream.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log error (in a real app, use ILogger)
                    Console.WriteLine($"Stream error: {ex.Message}");
                }
                finally
                {
                    response.Close();
                }
            }
        }
    }
}
