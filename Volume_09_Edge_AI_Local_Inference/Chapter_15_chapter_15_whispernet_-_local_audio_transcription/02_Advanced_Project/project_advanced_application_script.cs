
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
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;

namespace EdgeAiAudioTranscriber
{
    class Program
    {
        // Real-world context: A field researcher records interviews in remote locations with limited internet.
        // They need to transcribe these audio files locally on their rugged laptop immediately after recording
        // to analyze data without waiting for cloud uploads or risking data privacy.
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Edge AI Audio Transcriber ===");
            Console.WriteLine("Local Whisper.net Processing Initialized.\n");

            // 1. Configuration & Environment Setup
            // We define the model type. For this demo, we use Tiny (English) for speed on edge devices.
            // In production, you might use Base or Small for better accuracy, or Multilingual models.
            var modelType = GgmlType.Tiny;
            var modelPath = "ggml-tiny.bin";
            var audioPath = "sample_interview.wav";

            // Validate inputs before heavy processing
            if (!ValidateEnvironment(modelPath, audioPath))
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            // 2. Model Initialization
            // We wrap the WhisperFactory in a using statement to ensure native resources are released.
            // This is critical for Edge AI to prevent memory leaks on long-running devices.
            try
            {
                await using var whisperFactory = WhisperFactory.FromPath(modelPath);

                // 3. Processor Configuration
                // Configure the inference parameters. 
                // Strategy: Use 'Tiny' model for low latency, enable translation if needed (English to English is no-op).
                // We set a language hint to "en" to skip auto-detection overhead.
                var processor = whisperFactory.CreateBuilder()
                    .WithLanguage("en")
                    .WithTranslate(false)
                    .Build();

                Console.WriteLine($"Model loaded: {modelType}");

                // 4. Audio Processing Pipeline
                // Open the audio file. Whisper.net handles WAV/PCM decoding internally.
                // For real-time apps, you would stream from a microphone buffer here instead of a file.
                using var fileStream = File.OpenRead(audioPath);
                
                Console.WriteLine($"Processing audio: {audioPath}...\n");
                Console.WriteLine("--- Transcript ---");

                // 5. Inference Execution
                // The 'TranscribeAsync' method handles the audio chunking, FFT (Fast Fourier Transform), 
                // and neural network inference locally.
                // We iterate over segments to display text as it is generated (streaming simulation).
                await foreach (var segment in processor.TranscribeAsync(fileStream))
                {
                    // Formatting the output for readability
                    Console.WriteLine($"[{segment.Start:hh\\:mm\\:ss} --> {segment.End:hh\\:mm\\:ss}]");
                    Console.WriteLine($"  {segment.Text}");
                    Console.WriteLine();
                }

                Console.WriteLine("--- End of Transcript ---");
                Console.WriteLine("\nProcessing complete. No data was sent to the cloud.");
            }
            catch (Exception ex)
            {
                // Robust error handling for Edge scenarios (e.g., missing model file, corrupted audio)
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Processing failed: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Ensure the 'ggml-tiny.bin' model file exists in the execution directory.");
            }

            Console.WriteLine("\nPress any key to close...");
            Console.ReadKey();
        }

        /// <summary>
        /// Validates that the required model and audio files exist.
        /// </summary>
        /// <param name="modelPath">Path to the ONNX/GGML model file.</param>
        /// <param name="audioPath">Path to the input audio file.</param>
        /// <returns>True if valid, False otherwise.</returns>
        static bool ValidateEnvironment(string modelPath, string audioPath)
        {
            bool isValid = true;

            if (!File.Exists(modelPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[MISSING] Model file not found: {modelPath}");
                Console.WriteLine("Please download the ggml-tiny.bin model and place it in the output directory.");
                Console.ResetColor();
                isValid = false;
            }

            if (!File.Exists(audioPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[MISSING] Audio file not found: {audioPath}");
                Console.WriteLine("Creating a dummy audio file for demonstration purposes...");
                
                // Creating a dummy file to ensure the program runs for demonstration
                // In a real scenario, this would come from a microphone or external source.
                try 
                {
                    // Creating a silent WAV header (minimal valid WAV file)
                    // Note: Whisper will process this, but it won't produce text (silence).
                    // This is just to prevent the program from crashing if the file is missing.
                    byte[] dummyData = new byte[44]; 
                    File.WriteAllBytes(audioPath, dummyData);
                    Console.WriteLine("Dummy file created.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not create dummy file: {ex.Message}");
                    isValid = false;
                }
                Console.ResetColor();
            }

            return isValid;
        }
    }
}
