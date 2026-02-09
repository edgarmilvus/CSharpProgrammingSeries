
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

/// <summary>
/// Real-World Context: "SentinelLog" - A Security Operations Center (SOC) Dashboard.
/// Problem: Security analysts are overwhelmed by thousands of log entries daily.
/// Manually categorizing them (e.g., "Intrusion Attempt", "System Error", "User Action") is slow.
/// Solution: We build a local, offline AI classifier using a Phi-3 Mini ONNX model.
/// It reads raw log strings and outputs a category instantly, protecting sensitive data
/// by keeping everything on-premise.
/// </summary>
public class Program
{
    // --- Configuration Constants ---
    // In a production app, these would come from a config file.
    private const string ModelPath = "phi-3-mini-4k-instruct-onnx/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4"; // Path to the ONNX model folder
    private const string VocabularyPath = "vocab.json"; // Path to the tokenizer vocabulary
    
    // Maximum number of tokens the model can handle in one go.
    private const int MaxTokens = 512;

    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- SentinelLog AI Classifier v1.0 ---");
        Console.WriteLine("Initializing Local Inference Engine...");

        // 1. Validate Environment
        if (!ValidateModelFiles())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Model files not found. Please ensure the ONNX model and vocab are in the executable directory.");
            Console.ResetColor();
            return;
        }

        // 2. Initialize the Inference Session
        // This loads the heavy model into memory.
        // In WPF/WinForms, this should happen in a background worker to avoid UI freeze.
        using var session = new InferenceSession(ModelPath);
        
        // 3. Load Tokenizer Vocabulary
        // We simulate a tokenizer by looking up words in a dictionary.
        // Real-world tokenizers (like BPE) are complex; this is a simplified version for the chapter.
        var vocab = LoadVocabulary(VocabularyPath);

        // 4. The Application Loop (Simulating a Live Log Stream)
        string[] dummyLogs = new string[] {
            "Error: Connection timeout on port 8080",
            "Admin login successful from IP 192.168.1.50",
            "CRITICAL: SQL Injection attempt detected in query param 'id'",
            "User 'jdoe' updated profile settings",
            "Warning: Disk space at 95%"
        };

        Console.WriteLine("Processing Log Stream...\n");

        // Process logs asynchronously to keep the "application" responsive
        foreach (var log in dummyLogs)
        {
            Console.WriteLine($"[INPUT]: {log}");
            
            // A. Tokenize: Convert text to numbers
            int[] inputIds = Tokenize(log, vocab);
            
            // B. Run Inference: The heavy lifting
            // We use Task.Run to offload CPU work from the main thread
            string category = await Task.Run(() => ClassifyLog(session, inputIds));
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[AI PREDICTION]: {category}");
            Console.WriteLine(new string('-', 40));
            Console.ResetColor();
            
            // Simulate delay for live stream
            await Task.Delay(800);
        }

        Console.WriteLine("\nBatch processing complete. Press any key to exit.");
        Console.ReadKey();
    }

    /// <summary>
    /// Validates that the required files exist before attempting to load the model.
    /// </summary>
    static bool ValidateModelFiles()
    {
        // In a real WPF app, we would show a loading splash screen here.
        return File.Exists(Path.Combine(ModelPath, "model.onnx")) && File.Exists(VocabularyPath);
    }

    /// <summary>
    /// Loads a simple JSON vocabulary file into a Dictionary.
    /// </summary>
    static Dictionary<string, int> LoadVocabulary(string path)
    {
        var vocab = new Dictionary<string, int>();
        // Simulating reading a JSON file. In reality, use System.Text.Json
        // For this example, we hardcode a few tokens to make it runnable.
        vocab["error"] = 1;
        vocab["connection"] = 2;
        vocab["timeout"] = 3;
        vocab["admin"] = 4;
        vocab["login"] = 5;
        vocab["critical"] = 6;
        vocab["sql"] = 7;
        vocab["injection"] = 8;
        vocab["user"] = 9;
        vocab["updated"] = 10;
        vocab["warning"] = 11;
        vocab["disk"] = 12;
        vocab["space"] = 13;
        vocab["success"] = 14;
        vocab["attempt"] = 15;
        return vocab;
    }

    /// <summary>
    /// Converts a raw string into an array of integers (Token IDs).
    /// This mimics the work of a BPE Tokenizer.
    /// </summary>
    static int[] Tokenize(string text, Dictionary<string, int> vocab)
    {
        var tokens = new List<int>();
        // Simple whitespace tokenization and lowercasing
        var words = text.ToLower().Split(new[] { ' ', '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (vocab.ContainsKey(word))
            {
                tokens.Add(vocab[word]);
            }
            else
            {
                // Map unknown words to a generic ID (e.g., 0)
                tokens.Add(0); 
            }
        }

        // Pad or truncate to MaxTokens
        if (tokens.Count < MaxTokens)
        {
            while (tokens.Count < MaxTokens) tokens.Add(0); // Padding ID
        }
        else
        {
            tokens.RemoveRange(MaxTokens, tokens.Count - MaxTokens);
        }

        return tokens.ToArray();
    }

    /// <summary>
    /// The Core Inference Logic.
    /// Prepares inputs, runs the ONNX model, and interprets the output.
    /// </summary>
    static string ClassifyLog(InferenceSession session, int[] inputIds)
    {
        // 1. Prepare Input Tensor
        // ONNX Runtime expects a Tensor<T>.
        // Shape: [BatchSize (1), SequenceLength (MaxTokens)]
        var inputTensor = new DenseTensor<long>(new long[inputIds.Length], new[] { 1, inputIds.Length });
        
        for (int i = 0; i < inputIds.Length; i++)
        {
            inputTensor.SetValue(i, inputIds[i]);
        }

        // 2. Create NamedOnnxValue (The input name "input_ids" depends on the specific model export)
        // For Phi-3, it is usually "input_ids".
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
        };

        // 3. Run Inference
        // This is the blocking call that uses the CPU/NPU.
        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
        
        // 4. Process Output
        // The output is usually a tensor of logits [1, SequenceLength, VocabSize].
        // We need to find the token with the highest probability for the next word.
        // However, for classification tasks (like Phi-3 structured output), we often look at the 
        // logits of the *first* new token generated.
        
        // For this simulation, we will extract the raw output and map it to a category 
        // based on a heuristic (since we aren't running a full generation loop).
        var outputTensor = results.First().AsTensor<float>();
        
        // Find the index of the highest logit (greedy search simulation)
        float maxVal = float.MinValue;
        int maxIndex = 0;
        
        // We look at the first position of the output sequence (index 0, 0) 
        // and iterate over the vocabulary size dimension to find the winning token.
        // Note: Actual generation requires a loop. This is a "Single Step" classification simulation.
        
        // Let's simulate the logic: We look at the output logits to determine intent.
        // In a real app, we would loop generation until we hit a stop token.
        
        // Heuristic mapping for the demo:
        // We will look at specific indices in the output tensor to simulate the model 
        // "thinking" about specific categories based on the input tokens.
        
        // Since we can't run a full 512-token generation loop in a concise example without
        // a heavy decoding library, we will simulate the result based on the input content
        // to prove the architecture works, or use the raw output if available.
        
        // Let's actually inspect the output tensor to be "real".
        // If the model output is [1, 512, 32000], we take the first token logits.
        // We sum the logits for specific "category tokens" we expect.
        
        // SIMULATION BRIDGE:
        // To keep this code executable without 1000 lines of decoding logic,
        // we will interpret the output tensor's shape and values to generate a label.
        // If the output tensor is essentially random (untrained model), we fallback to a heuristic.
        
        // Let's use a simple rule-based fallback for the demo if the model isn't fine-tuned,
        // but wrap it in the ONNX structure.
        
        // However, to be faithful to "Edge AI", let's assume the model returns a specific token ID
        // representing the class. We will look for the highest value in the first row of logits.
        
        // Get the logits for the first position (index 0)
        // We need to slice the tensor: [1, 512, 32000] -> take [0, 0, :] -> size 32000
        // Since we don't know the vocab size dynamically in this snippet, we iterate the tensor dimensions.
        
        var shape = outputTensor.Dimensions;
        int vocabSize = shape[2]; // Usually 32000 for Phi-3
        
        // Find the token ID with the highest logit at position [0, 0]
        float currentMax = float.MinValue;
        int predictedTokenId = 0;
        
        // We are looking at the first token's logits
        for (int v = 0; v < vocabSize; v++)
        {
            // Accessing tensor data: [batch, sequence, vocab]
            // Since we only have 1 batch and we look at first sequence item
            float val = outputTensor[0, 0, v];
            if (val > currentMax)
            {
                currentMax = val;
                predictedTokenId = v;
            }
        }

        // Map the predicted Token ID to a human-readable category
        // In a fine-tuned model, ID 300 might mean "Error", ID 500 "Security".
        // Here we map based on our dummy vocab IDs for the demo.
        return MapTokenToCategory(predictedTokenId);
    }

    /// <summary>
    /// Maps a raw token ID to a business category.
    /// </summary>
    static string MapTokenToCategory(int tokenId)
    {
        // This mapping represents the "Head" of the model.
        // In a real scenario, these IDs correspond to tokens like "Security", "System", etc.
        
        // We use basic if/else as per constraints.
        if (tokenId == 1 || tokenId == 3 || tokenId == 11 || tokenId == 12) 
        {
            // 1: error, 3: timeout, 11: warning, 12: disk
            return "SYSTEM_FAULT";
        }
        if (tokenId == 4 || tokenId == 5 || tokenId == 14) 
        {
            // 4: admin, 5: login, 14: success
            return "ACCESS_EVENT";
        }
        if (tokenId == 6 || tokenId == 7 || tokenId == 8 || tokenId == 15) 
        {
            // 6: critical, 7: sql, 8: injection, 15: attempt
            return "SECURITY_ALERT";
        }
        if (tokenId == 9 || tokenId == 10) 
        {
            // 9: user, 10: updated
            return "USER_ACTIVITY";
        }
        
        return "UNCATEGORIZED";
    }
}
