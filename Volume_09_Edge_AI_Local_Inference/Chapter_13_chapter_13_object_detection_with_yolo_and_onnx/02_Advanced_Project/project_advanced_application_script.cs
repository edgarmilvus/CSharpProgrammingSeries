
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace SmartRetailMonitor
{
    class Program
    {
        // --- CONFIGURATION & CONSTANTS ---
        // Real-world context: A retail store manager wants to automate shelf monitoring.
        // They have a specific camera feed and a YOLOv8 model trained to detect "Bottle" and "Can".
        const string ModelPath = "models/yolov8_shelf_detector.onnx";
        const string ImagePath = "images/shelf_snapshot.jpg";
        const float ScoreThreshold = 0.50f; // Confidence level to consider a detection valid
        const float NmsThreshold = 0.45f;   // Non-Maximum Suppression threshold to avoid duplicate boxes
        const int InputSize = 640;          // YOLO models usually require square input (e.g., 640x640)

        // Class names specific to our custom trained model
        static readonly string[] ClassNames = { "Bottle", "Can" };

        static void Main(string[] args)
        {
            Console.WriteLine("--- Smart Retail Inventory Monitor ---");
            Console.WriteLine("Initializing ONNX Runtime Session...");

            if (!File.Exists(ModelPath))
            {
                Console.WriteLine($"[Error] Model file not found at: {ModelPath}");
                return;
            }

            // 1. LOAD MODEL
            // We use the InferenceSession from the ONNX Runtime C# API.
            // This loads the graph, weights, and prepares the execution provider (CPU/GPU).
            using var session = new InferenceSession(ModelPath);

            // 2. PREPARE IMAGE
            // Load the bitmap and convert it into a Tensor<float> format that the model understands.
            // YOLO expects normalized pixel values (0-1) and specific channel ordering (RGB).
            if (!File.Exists(ImagePath))
            {
                Console.WriteLine($"[Error] Image file not found at: {ImagePath}");
                return;
            }

            Console.WriteLine("Processing image...");
            using var originalBitmap = new Bitmap(ImagePath);
            
            // Create the input tensor (Shape: [1, 3, 640, 640])
            var inputTensor = PreprocessImage(originalBitmap, InputSize);
            
            // Define the input name (usually 'images' in YOLO exports)
            var inputName = session.InputMetadata.Keys.GetEnumerator().Current;
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputTensor) };

            // 3. RUN INFERENCE
            Console.WriteLine("Running inference...");
            using var results = session.Run(inputs);
            
            // 4. POST-PROCESSING
            // The raw output is a tensor of shape [1, 84, 8400] (for YOLOv8).
            // 84 = 4 box coords (cx, cy, w, h) + 80 class probs (if COCO) or fewer for custom models.
            // We need to filter these raw predictions into usable Detections.
            var outputTensor = results[0].AsTensor<float>();
            var detections = ParseOutput(outputTensor, originalBitmap.Width, originalBitmap.Height);

            // 5. RENDERING & REPORTING
            // In a real app, we might draw using SkiaSharp. Here we use System.Drawing for simplicity
            // and generate a text report + a visualized file.
            Console.WriteLine($"\nDetected {detections.Count} items:");
            
            using (var g = Graphics.FromImage(originalBitmap))
            {
                // Draw transparent boxes
                using var pen = new Pen(Color.Red, 3);
                using var font = new Font("Arial", 12);
                using var brush = new SolidBrush(Color.FromArgb(128, 255, 0, 0)); // Semi-transparent red

                foreach (var det in detections)
                {
                    // Draw Box
                    g.DrawRectangle(pen, det.Box);
                    g.FillRectangle(brush, det.Box);

                    // Draw Label
                    string label = $"{ClassNames[det.ClassId]}: {det.Confidence:P1}";
                    g.DrawString(label, font, Brushes.Yellow, det.Box.X, det.Box.Y - 15);

                    Console.WriteLine($" - {label} at ({det.Box.X}, {det.Box.Y})");
                }
            }

            string resultPath = "shelf_analysis_result.jpg";
            originalBitmap.Save(resultPath, ImageFormat.Jpeg);
            Console.WriteLine($"\nAnalysis complete. Visual result saved to: {resultPath}");
        }

        // --- HELPER METHODS ---

        /// <summary>
        /// Converts a Bitmap into a normalized Float Tensor.
        /// Resize -> Pad to Square -> Normalize (0-1) -> Transpose CHW.
        /// </summary>
        static DenseTensor<float> PreprocessImage(Bitmap image, int targetSize)
        {
            // Calculate scaling ratio while maintaining aspect ratio
            float widthRatio = (float)targetSize / image.Width;
            float heightRatio = (float)targetSize / image.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            int newWidth = (int)(image.Width * scale);
            int newHeight = (int)(image.Height * scale);

            // Resize the image
            using var resized = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(resized))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            // Create the tensor [1, 3, targetSize, targetSize]
            // We pad the remaining space with gray (128) or black (0), but YOLO usually pads with 114.
            var tensor = new DenseTensor<float>(new[] { 1, 3, targetSize, targetSize });
            
            // Lock bits for fast access
            var bmpData = resized.LockBits(new Rectangle(0, 0, newWidth, newHeight), 
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            
            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int bytesPerPixel = 3;

                // Calculate padding to center the image in the square target
                int padX = (targetSize - newWidth) / 2;
                int padY = (targetSize - newHeight) / 2;

                for (int y = 0; y < targetSize; y++)
                {
                    for (int x = 0; x < targetSize; x++)
                    {
                        // Check if pixel is within the padded image area
                        if (x >= padX && x < padX + newWidth && y >= padY && y < padY + newHeight)
                        {
                            // Coordinates in the resized source image
                            int srcX = x - padX;
                            int srcY = y - padY;

                            // Get pixel pointer (BGR order in Bitmap)
                            byte* pixel = ptr + (srcY * bmpData.Stride) + (srcX * bytesPerPixel);
                            
                            // Normalize to 0-1 and store in Tensor (RGB order)
                            // Input format for YOLO: C, H, W
                            tensor[0, 0, y, x] = pixel[2] / 255.0f; // R
                            tensor[0, 1, y, x] = pixel[1] / 255.0f; // G
                            tensor[0, 2, y, x] = pixel[0] / 255.0f; // B
                        }
                        else
                        {
                            // Padding value (114/255 ≈ 0.447)
                            float padVal = 114.0f / 255.0f;
                            tensor[0, 0, y, x] = padVal;
                            tensor[0, 1, y, x] = padVal;
                            tensor[0, 2, y, x] = padVal;
                        }
                    }
                }
            }
            resized.UnlockBits(bmpData);
            return tensor;
        }

        /// <summary>
        /// Parses the raw YOLO output tensor into a list of filtered Detections.
        /// Handles NMS and mapping back to original image coordinates.
        /// </summary>
        static List<Detection> ParseOutput(Tensor<float> output, int origW, int origH)
        {
            var detections = new List<Detection>();
            
            // YOLOv8 Output Shape: [1, 4 + NumClasses, 8400] (usually flattened)
            // We need to determine dimensions dynamically.
            // Note: This implementation assumes a flattened structure typical of ONNX exports.
            // Dimensions: [Batch, Channels, Anchors] -> [1, 84, 8400] (if 80 classes)
            
            int dimensions = output.Dimensions[1]; // e.g., 84 (4 coords + 80 classes)
            int anchors = output.Dimensions[2];    // e.g., 8400

            // We need to iterate through all 8400 predictions
            for (int i = 0; i < anchors; i++)
            {
                // Calculate the offset in the flattened array
                // The structure is usually: [Batch, Channels, Anchors] -> flattened
                // Accessing: output[0, channel, anchor]
                
                // Get Box Coordinates (First 4 values)
                // YOLO outputs are usually: cx, cy, w, h (center x/y, width, height)
                // These are normalized (0.0 to 1.0) relative to the input size (640)
                float x = output[0, 0, i];
                float y = output[0, 1, i];
                float w = output[0, 2, i];
                float h = output[0, 3, i];

                // Find the class with the highest score
                float maxScore = 0f;
                int classId = -1;

                // Loop through classes (starting from index 4)
                for (int c = 4; c < dimensions; c++)
                {
                    float score = output[0, c, i];
                    if (score > maxScore)
                    {
                        maxScore = score;
                        classId = c - 4; // Offset because first 4 are coords
                    }
                }

                // Filter by threshold
                if (maxScore < ScoreThreshold) continue;

                // Convert normalized coords to original image coords
                // We also need to account for the resizing/padding logic in PreprocessImage
                // However, for simplicity in this specific script, we will map directly to the 640 input space
                // and then scale to original.
                
                // Note: A full production solution would undo the padding (letterboxing) exactly.
                // Here we assume the detection is on the content area and we scale it.
                
                float scale = Math.Min((float)InputSize / origW, (float)InputSize / origH);
                float padX = (InputSize - origW * scale) / 2;
                float padY = (InputSize - origH * scale) / 2;

                // Map center coordinates
                float centerX = (x * InputSize - padX) / scale;
                float centerY = (y * InputSize - padY) / scale;
                
                // Map dimensions
                float width = (w * InputSize) / scale;
                float height = (h * InputSize) / scale;

                // Create Rectangle (Top-Left based)
                int boxX = (int)(centerX - (width / 2));
                int boxY = (int)(centerY - (height / 2));
                int boxW = (int)width;
                int boxH = (int)height;

                // Boundary checks
                if (boxX < 0) boxX = 0;
                if (boxY < 0) boxY = 0;
                if (boxX + boxW > origW) boxW = origW - boxX;
                if (boxY + boxH > origH) boxH = origH - boxY;

                if (boxW <= 0 || boxH <= 0) continue;

                detections.Add(new Detection
                {
                    Box = new Rectangle(boxX, boxY, boxW, boxH),
                    ClassId = classId,
                    Confidence = maxScore
                });
            }

            // 6. APPLY NON-MAXIMUM SUPPRESSION (NMS)
            // Sort by confidence (descending)
            // Note: Using simple loops instead of LINQ to adhere to constraints
            for (int i = 0; i < detections.Count - 1; i++)
            {
                for (int j = i + 1; j < detections.Count; j++)
                {
                    if (detections[i].Confidence < detections[j].Confidence)
                    {
                        var temp = detections[i];
                        detections[i] = detections[j];
                        detections[j] = temp;
                    }
                }
            }

            // Filter overlapping boxes
            var keep = new List<Detection>();
            for (int i = 0; i < detections.Count; i++)
            {
                bool keepBox = true;
                for (int j = 0; j < keep.Count; j++)
                {
                    float iou = CalculateIoU(detections[i].Box, keep[j].Box);
                    if (iou > NmsThreshold && detections[i].ClassId == keep[j].ClassId)
                    {
                        keepBox = false;
                        break;
                    }
                }
                if (keepBox) keep.Add(detections[i]);
            }

            return keep;
        }

        /// <summary>
        /// Calculates Intersection over Union (IoU) for two rectangles.
        /// Used by NMS to determine overlap.
        /// </summary>
        static float CalculateIoU(Rectangle a, Rectangle b)
        {
            int x1 = Math.Max(a.X, b.X);
            int y1 = Math.Max(a.Y, b.Y);
            int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            int intersectionArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            int unionArea = a.Width * a.Height + b.Width * b.Height - intersectionArea;

            if (unionArea == 0) return 0;
            return (float)intersectionArea / unionArea;
        }
    }

    // Data structure for a single detection
    class Detection
    {
        public Rectangle Box { get; set; }
        public int ClassId { get; set; }
        public float Confidence { get; set; }
    }
}
