
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ImageProcessor
{
    public class ImageItem
    {
        public string Filename { get; set; }
        public bool IsSensitive { get; set; }
    }

    public static async Task RunAsync()
    {
        // 1. Data Setup
        var itemsToProcess = new List<ImageItem>
        {
            new ImageItem { Filename = "vacation_photo.jpg", IsSensitive = false },
            new ImageItem { Filename = "passport_scan.png", IsSensitive = true },
            new ImageItem { Filename = "cat_meme.jpg", IsSensitive = false },
            new ImageItem { Filename = "credit_card_front.jpg", IsSensitive = true },
            new ImageItem { Filename = "receipt.jpg", IsSensitive = false }
        };

        // 2. Simulated User Approval Queue
        // True = Approve, False = Reject
        var approvalQueue = new Queue<bool>();
        approvalQueue.Enqueue(true);  // Approve passport
        approvalQueue.Enqueue(false); // Reject credit card

        Console.WriteLine("--- Starting Image Processing Workflow ---\n");

        // 3. The Loop
        foreach (var item in itemsToProcess)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Inspecting: {item.Filename}");

            if (!item.IsSensitive)
            {
                // Automatic Processing
                Console.WriteLine($"  -> Status: Safe. Auto-processing...");
                await ProcessImage(item); // Simulate work
            }
            else
            {
                // Interruptible / Human-in-the-Loop
                Console.WriteLine($"  -> Status: SENSITIVE. Pausing for approval...");

                // Simulate the "Wait" for external input
                await Task.Delay(1000); // Represents time passing while waiting for user

                // Get simulated decision
                if (approvalQueue.Count > 0)
                {
                    bool isApproved = approvalQueue.Dequeue();
                    
                    if (isApproved)
                    {
                        Console.WriteLine($"  -> User Approved. Processing...");
                        await ProcessImage(item);
                    }
                    else
                    {
                        Console.WriteLine($"  -> User Rejected. Skipped.");
                    }
                }
                else
                {
                    Console.WriteLine($"  -> No approval received (timeout). Skipped.");
                }
            }
        }

        Console.WriteLine("\n--- Workflow Complete ---");
    }

    private static async Task ProcessImage(ImageItem item)
    {
        // Simulate async processing time
        await Task.Delay(500);
        Console.WriteLine($"     Done processing {item.Filename}.");
    }
}
