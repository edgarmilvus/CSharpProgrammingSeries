
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNativeAgentExample
{
    // Represents the data structure for an incoming request.
    // Using 'record' for immutable data transfer objects (DTOs).
    public record InferenceRequest(string InputText);

    // Represents the response from the AI model.
    public record InferenceResponse(string Sentiment, double Confidence, long ProcessingTimeMs);

    // The core AI Agent logic. In a real scenario, this would wrap a TensorFlow.NET or ONNX model.
    // Here, we simulate the inference process.
    public class SentimentAnalysisAgent
    {
        // Simulates a complex AI model inference.
        // In a real containerized environment, this method would load a model from disk.
        public async Task<InferenceResponse> AnalyzeAsync(InferenceRequest request, CancellationToken ct)
        {
            var startTime = System.Diagnostics.Stopwatch.GetTimestamp();
            
            // Simulate network latency or GPU processing time.
            await Task.Delay(new Random().Next(50, 200), ct);

            // Simple heuristic simulation for "Hello World" purposes.
            // Real AI would use matrix multiplication here.
            var text = request.InputText.ToLower();
            double confidence = 0.5;
            string sentiment = "Neutral";

            if (text.Contains("happy") || text.Contains("great"))
            {
                sentiment = "Positive";
                confidence = 0.95;
            }
            else if (text.Contains("sad") || text.Contains("bad"))
            {
                sentiment = "Negative";
                confidence = 0.92;
            }

            var elapsedMs = System.Diagnostics.Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

            return new InferenceResponse(sentiment, confidence, (long)elapsedMs);
        }
    }

    // The HTTP Server acting as the microservice endpoint.
    public class AgentServer
    {
        private readonly HttpListener _listener;
        private readonly SentimentAnalysisAgent _agent;
        private readonly CancellationTokenSource _cts;

        public AgentServer(string url)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _agent = new SentimentAnalysisAgent();
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine($"[AgentServer] Listening on {_listener.Prefixes.First()}...");

            // Use a TaskCompletionSource to handle graceful shutdown signals.
            var shutdownSignal = new TaskCompletionSource<bool>();

            // Register a console cancel key press to trigger shutdown.
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                _cts.Cancel();
                shutdownSignal.TrySetResult(true);
            };

            // Main server loop using asynchronous processing.
            // We accept connections and process them concurrently.
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    // Asynchronously wait for an incoming request.
                    var context = await _listener.GetContextAsync().WaitAsync(_cts.Token);
                    
                    // Fire and forget, but track the task to observe exceptions.
                    // In a high-throughput system, you might use a bounded channel or SemaphoreSlim here
                    // to limit concurrent requests and prevent resource exhaustion.
                    _ = Task.Run(() => HandleRequestAsync(context, _cts.Token));
                }
                catch (OperationCanceledException)
                {
                    break; // Graceful exit
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Accepting connection: {ex.Message}");
                }
            }

            await shutdownSignal.Task;
            Console.WriteLine("[AgentServer] Stopped.");
        }

        private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken ct)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                // Only allow POST requests for inference.
                if (request.HttpMethod != "POST")
                {
                    response.StatusCode = 405;
                    response.Close();
                    return;
                }

                // Read the request body asynchronously.
                string body;
                using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                {
                    body = await reader.ReadToEndAsync();
                }

                // Deserialize the JSON payload using System.Text.Json.
                var inferenceRequest = JsonSerializer.Deserialize<InferenceRequest>(body);

                if (inferenceRequest == null || string.IsNullOrWhiteSpace(inferenceRequest.InputText))
                {
                    response.StatusCode = 400; // Bad Request
                    var errorBytes = Encoding.UTF8.GetBytes("Invalid input text.");
                    await response.OutputStream.WriteAsync(errorBytes, 0, errorBytes.Length, ct);
                    response.Close();
                    return;
                }

                // Perform the AI inference.
                var result = await _agent.AnalyzeAsync(inferenceRequest, ct);

                // Serialize the result back to JSON.
                var jsonResponse = JsonSerializer.Serialize(result);
                var buffer = Encoding.UTF8.GetBytes(jsonResponse);

                // Send the response.
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, ct);
            }
            catch (OperationCanceledException)
            {
                // Handle timeout or shutdown during processing.
                if (!response.OutputStream.CanWrite) return;
                response.StatusCode = 503; // Service Unavailable
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Processing request: {ex.Message}");
                if (!response.OutputStream.CanWrite)
                {
                    response.StatusCode = 500;
                    response.Close();
                }
            }
            finally
            {
                // Ensure the response stream is closed to release resources.
                response.Close();
            }
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            _listener.Stop();
            _listener.Close();
        }
    }

    // Main entry point.
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure the server to listen on localhost port 8080.
            // In a containerized setup, this port would be mapped to the host.
            var server = new AgentServer("http://localhost:8080/");
            
            await server.StartAsync();
        }
    }
}
