
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace WhisperLocalDemo
{
    class Program
    {
        // The main entry point of the application.
        static async Task Main(string[] args)
        {
            // 1. Define the path to the audio file we want to transcribe.
            //    In a real app, this might come from a microphone stream or a file picker.
            string audioFilePath = "sample.wav";

            // Check if the audio file exists before proceeding.
            if (!File.Exists(audioFilePath))
            {
                Console.WriteLine($"Error: Audio file not found at '{audioFilePath}'.");
                Console.WriteLine("Please ensure a 'sample.wav' file exists in the execution directory.");
                return;
            }

            Console.WriteLine($"Starting transcription for: {audioFilePath}");

            // 2. Define the path where the Whisper model file will be downloaded.
            //    Whisper.net handles downloading the model if it's not present.
            //    We'll use the 'Tiny' model for speed and low resource usage (ideal for 'Hello World').
            string modelPath = "ggml-tiny.bin";

            // 3. Initialize the WhisperFactory.
            //    This is the central factory class that manages the model loading and inference engine.
            //    It abstracts away the complexity of the underlying ONNX runtime or Ggml backend.
            using var whisperFactory = WhisperFactory.FromPath(modelPath);

            // 4. Build the processor.
            //    We configure the processing parameters here. This is where we define:
            //    - The language (optional, but helps performance if known).
            //    - The strategy for handling partial results (e.g., real-time vs. full file).
            //    - Callbacks for when a segment of text is fully transcribed.
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("en") // Explicitly set English to improve accuracy and speed.
                .WithSegmentEventHandler(segment => 
                {
                    // This callback is triggered whenever Whisper finalizes a segment of text.
                    // A "segment" is typically a sentence or a logical chunk of speech.
                    Console.WriteLine($"[{segment.Start}->{segment.End}]: {segment.Text}");
                })
                .Build();

            // 5. Open the audio file and process it.
            //    We use a FileStream to read the audio data.
            //    Whisper.net expects the audio data to be in a specific format (usually 16-bit PCM, 16kHz, mono).
            //    The library handles the internal buffering and chunking automatically.
            using var fileStream = File.OpenRead(audioFilePath);
            
            // The 'ProcessAsync' method streams the audio data to the model and invokes the callbacks.
            await processor.ProcessAsync(fileStream);

            Console.WriteLine("Transcription completed.");
        }
    }
}
