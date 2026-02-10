
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LowLatencyAudioStreamSimulator
{
    /// <summary>
    /// Real-World Context: 
    /// This application simulates a "Voice Assistant Gateway" that receives raw audio chunks 
    /// from a client (e.g., a mobile app recording a voice command) via WebSocket. 
    /// It processes these chunks in real-time to detect silence and trigger an AI model 
    /// inference, mimicking the behavior of a system like Alexa or Siri.
    /// </summary>
    class Program
    {
        // Configuration constants for the audio stream
        private const int AudioChunkSize = 1024; // Bytes per WebSocket message
        private const int SilenceThresholdMs = 2000; // 2 seconds of silence triggers processing
        private const int SampleRate = 16000; // Standard for speech recognition
        private const int BytesPerSample = 2; // 16-bit audio
        private const int MaxBufferSize = SampleRate * BytesPerSample * 10; // 10 seconds buffer max

        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Low-Latency Audio Stream Simulator...");
            
            // Simulate a WebSocket connection (In a real app, this comes from HttpContext.WebSockets)
            using (var clientSocket = new ClientWebSocket())
            {
                try
                {
                    // Connect to a mock server endpoint (simulating the ASP.NET Core backend)
                    var serverUri = new Uri("ws://localhost:5000/audio-stream");
                    Console.WriteLine($"Connecting to {serverUri}...");
                    
                    // Note: Since we cannot run a real server in a single console app easily, 
                    // we will simulate the server logic in a separate task below.
                    // In a real scenario, this connects to an actual ASP.NET Core WebSocket middleware.
                    // await clientSocket.ConnectAsync(serverUri, CancellationToken.None);
                    
                    // SIMULATION MODE: We will act as both Client and Server for this demo
                    // to demonstrate the logic without needing a separate running instance.
                    await RunSimulationAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Simulates the full lifecycle of a low-latency audio stream.
        /// 1. Generates synthetic audio chunks (noise + silence).
        /// 2. Buffers them efficiently.
        /// 3. Detects silence to trigger AI processing.
        /// </summary>
        private static async Task RunSimulationAsync()
        {
            // 1. Setup the buffer for incoming audio data
            // We use a MemoryStream to act as a dynamic circular buffer
            var audioBuffer = new MemoryStream();
            var cts = new CancellationTokenSource();
            
            Console.WriteLine("\n--- Starting Audio Stream Simulation ---");
            Console.WriteLine("Scenario: User speaks 'Hello AI', pauses, then speaks 'Turn on lights'.");
            Console.WriteLine("Goal: Detect silence gap and process commands separately.\n");

            // 2. Simulate Receiving Data Loop
            // In a real ASP.NET Core app, this is the `while` loop inside `await webSocket.ReceiveAsync(...)`
            int silenceCounter = 0;
            bool isCurrentlySpeaking = false;
            long lastVoiceActivityTime = DateTime.Now.Ticks;

            // Simulate 50 chunks of data
            for (int i = 0; i < 50; i++)
            {
                // Simulate network delay (low latency requirement)
                await Task.Delay(20); 

                // Generate synthetic audio data
                // Alternating: 10 chunks of voice, 10 chunks of silence, 10 chunks of voice
                byte[] chunk = GenerateAudioChunk(i, 10, 20); 
                
                // Log received chunk
                Console.Write($"\r[Chunk {i}] Received {chunk.Length} bytes. Status: {(IsSilence(chunk) ? "Silence" : "Voice   ")}");

                // 3. Buffer Management
                // Check if buffer exceeds max size (protect against memory overflow)
                if (audioBuffer.Length + chunk.Length > MaxBufferSize)
                {
                    Console.WriteLine("\n[WARNING] Buffer overflow. Draining old data.");
                    audioBuffer.SetLength(0); // Reset buffer (simplified logic)
                }

                // Write chunk to buffer
                await audioBuffer.WriteAsync(chunk, 0, chunk.Length);

                // 4. Silence Detection Logic
                // If chunk is silence, increment counter. If voice, reset counter.
                if (IsSilence(chunk))
                {
                    silenceCounter++;
                    if (isCurrentlySpeaking)
                    {
                        // Just transitioned to silence
                        Console.WriteLine("\n[EVENT] Voice activity ended. Starting silence timer...");
                        isCurrentlySpeaking = false;
                    }
                }
                else
                {
                    silenceCounter = 0;
                    isCurrentlySpeaking = true;
                    lastVoiceActivityTime = DateTime.Now.Ticks;
                }

                // 5. Trigger AI Processing
                // If silence persists for threshold, process the buffered audio
                if (silenceCounter >= SilenceThresholdMs / 20 && audioBuffer.Length > 0)
                {
                    Console.WriteLine($"\n[TRIGGER] Silence detected for {SilenceThresholdMs}ms. Triggering AI Model...");
                    
                    // Copy buffer to process
                    byte[] audioToProcess = audioBuffer.ToArray();
                    
                    // Offload to AI Processor (Simulated)
                    await ProcessWithAIModelAsync(audioToProcess);
                    
                    // Clear buffer for next command
                    audioBuffer.SetLength(0);
                    silenceCounter = 0;
                    isCurrentlySpeaking = false;
                }
            }

            Console.WriteLine("\n\n--- Simulation Complete ---");
        }

        /// <summary>
        /// Simulates the AI Model inference step.
        /// In a real scenario, this would send the byte array to a TensorFlow.NET or ONNX model.
        /// </summary>
        private static async Task ProcessWithAIModelAsync(byte[] audioData)
        {
            Console.WriteLine($"    > Processing {audioData.Length} bytes of audio...");
            
            // Simulate CPU-bound heavy work (Model Inference)
            // We use Task.Run to avoid blocking the main thread (important for WebSocket keep-alive)
            await Task.Run(() =>
            {
                Thread.Sleep(500); // Simulate 500ms inference time
                
                // Mock Result
                string command = audioData.Length > 500 ? "Command: 'Turn on lights'" : "Command: 'Hello AI'";
                Console.WriteLine($"    > AI Result: {command}");
            });
        }

        /// <summary>
        /// Helper: Generates mock audio data.
        /// Returns all zeros (silence) or random noise (voice).
        /// </summary>
        private static byte[] GenerateAudioChunk(int index, int voiceDuration, int silenceDuration)
        {
            int cycleLength = voiceDuration + silenceDuration;
            int positionInCycle = index % cycleLength;
            
            bool isVoice = positionInCycle < voiceDuration;
            int size = isVoice ? 1024 : 256; // Voice is louder (more data) for simulation

            byte[] data = new byte[size];
            
            if (isVoice)
            {
                // Fill with random noise (simulating voice wave)
                new Random().NextBytes(data);
            }
            else
            {
                // Fill with 0 (silence)
                Array.Fill<byte>(data, 0);
            }

            return data;
        }

        /// <summary>
        /// Helper: Checks if the audio chunk represents silence (all zeros).
        /// </summary>
        private static bool IsSilence(byte[] chunk)
        {
            foreach (byte b in chunk)
            {
                if (b != 0) return false;
            }
            return true;
        }
    }
}
