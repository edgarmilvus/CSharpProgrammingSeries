
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.IO;

public class ConversationStateManager
{
    // Delegate for fallback actions
    public delegate void FallbackAction(string message);

    public void SaveHybrid(ConversationContext ctx, string basePath)
    {
        string binaryPath = basePath + ".bin";
        string jsonPath = basePath + ".json";

        try
        {
            // Attempt Binary Save (Simulating the logic from Exercise 2)
            // Note: In a real scenario, we would call ctx.SaveToBinary(binaryPath);
            // Here we simulate a potential failure for demonstration.
            SimulateBinarySave(ctx, binaryPath);
            
            Console.WriteLine("Binary save successful.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Binary save failed: {ex.Message}");

            // Define a fallback action using a Lambda
            FallbackAction fallback = (msg) => 
            {
                Console.WriteLine($"Executing fallback: {msg}");
                // Perform the JSON save
                JsonTensorSerializer.SerializeToJson(new Tensor(), jsonPath); 
                // Note: We are adapting the example; in reality, you'd need a JSON serializer for ConversationContext.
                // For this exercise, we focus on the control flow pattern.
            };

            // Execute the fallback
            fallback("Switching to JSON serialization due to I/O error");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error: {ex.Message}");
        }
    }

    // Helper to simulate an error for demonstration
    private void SimulateBinarySave(ConversationContext ctx, string path)
    {
        // Simulate a 50% chance of failure to trigger the catch block
        if (new Random().Next(0, 2) == 0)
        {
            throw new IOException("Simulated Disk Write Failure");
        }
        // Actual write logic would go here
    }
}

public class HybridManagerTest
{
    public static void Run()
    {
        var manager = new ConversationStateManager();
        var ctx = new ConversationContext("session-hybrid");
        
        // Run multiple times to see both success and failure paths
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"--- Attempt {i+1} ---");
            manager.SaveHybrid(ctx, "hybrid_context");
        }
    }
}
