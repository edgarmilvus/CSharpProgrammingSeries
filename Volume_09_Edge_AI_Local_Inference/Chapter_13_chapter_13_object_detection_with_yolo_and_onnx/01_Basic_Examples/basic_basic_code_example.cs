
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace YoloHelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. SETUP: Define paths and parameters
            // In a real scenario, these would be command-line arguments or config files.
            string modelPath = "yolov8n.onnx"; // A small, fast YOLOv8 Nano model
            string imagePath = "input.jpg";    // A sample image to detect objects in
            string outputPath = "output.jpg";  // Where to save the annotated image

            Console.WriteLine($"Loading YOLO model from: {modelPath}");
            
            // 2. LOAD MODEL: Initialize the ONNX Runtime Inference Session
            // We use the CPU execution provider for simplicity on any machine.
            var sessionOptions = new SessionOptions();
            sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;
            
            using var session = new InferenceSession(modelPath, sessionOptions);
            
            // 3. PRE-PROCESS IMAGE: Convert image to the tensor format YOLO expects
            // YOLOv8 expects a 640x640 RGB image normalized to 0-1.
            var inputTensor = PreProcessImage(imagePath, out int originalWidth, out int originalHeight);
            
            // 4. PREPARE INPUTS: Create the NamedOnnxValue for the session
            // Note: YOLOv8 uses 'images' as the input name. Check your specific model with Netron.
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", inputTensor)
            };

            Console.WriteLine("Running inference...");
            
            // 5. RUN INFERENCE: Execute the model
            using var results = session.Run(inputs);
            
            // 6. POST-PROCESS: Extract raw output and apply Non-Maximum Suppression (NMS)
            // YOLOv8 output shape is usually (1, 4 + num_classes, 8400) -> Transposed to (1, 8400, 84)
            var detections = PostProcessYoloV8(results, originalWidth, originalHeight);
            
            // 7. RENDER: Draw bounding boxes on the original image
            RenderBoxes(imagePath, outputPath, detections);
            
            Console.WriteLine($"Detection complete. Saved to {outputPath}");
            Console.WriteLine($"Found {detections.Count} objects.");
        }

        // --- HELPER METHODS ---

        static DenseTensor<float> PreProcessImage(string imagePath, out int originalWidth, out int originalHeight)
        {
            using Bitmap bitmap = new Bitmap(imagePath);
            originalWidth = bitmap.Width;
            originalHeight = bitmap.Height;

            // YOLOv8 standard input size
            const int targetSize = 640;

            // Resize image while preserving aspect ratio (letterbox padding)
            // For simplicity in this "Hello World", we will resize directly without padding
            // to keep the code concise, though padding is preferred for accuracy.
            using Bitmap resized = new Bitmap(bitmap, new Size(targetSize, targetSize));
            
            // Lock bits for fast memory access
            var data = resized.LockBits(new Rectangle(0, 0, targetSize, targetSize), 
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            
            try
            {
                // Create the tensor: Shape (1, 3, 640, 640) -> Batch, Channels, Height, Width
                var tensor = new DenseTensor<float>(new[] { 1, 3, targetSize, targetSize });
                
                // Copy pixels and normalize to 0-1
                // Note: In production, use Span<T> and unsafe code for maximum performance.
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0.ToPointer();
                    int bytesPerPixel = 3; // 24bpp

                    for (int y = 0; y < targetSize; y++)
                    {
                        for (int x = 0; x < targetSize; x++)
                        {
                            int rowOffset = y * data.Stride;
                            int colOffset = x * bytesPerPixel;

                            // Bitmap is usually BGR, but YOLO expects RGB.
                            // We will just grab R, G, B and normalize.
                            byte b = ptr[rowOffset + colOffset];
                            byte g = ptr[rowOffset + colOffset + 1];
                            byte r = ptr[rowOffset + colOffset + 2];

                            // Normalize (0-255 -> 0.0-1.0)
                            // Note: YOLOv8 usually requires ImageNet normalization, 
                            // but standard 0-1 works for many exported ONNX models.
                            // We assume RGB order here.
                            tensor[0, 0, y, x] = r / 255.0f;
                            tensor[0, 1, y, x] = g / 255.0f;
                            tensor[0, 2, y, x] = b / 255.0f;
                        }
                    }
                }
                return tensor;
            }
            finally
            {
                resized.UnlockBits(data);
            }
        }

        static List<Detection> PostProcessYoloV8(OrtValue results, int originalWidth, int originalHeight)
        {
            // 1. Extract the tensor data
            // Output shape for YOLOv8 is usually (1, 84, 8400) -> (Batch, Classes+Coords, Anchors)
            // We need to transpose or iterate accordingly.
            var tensor = results.AsTensor<float>();
            int dimensions = tensor.Dimensions[1]; // Should be 84 (4 coords + 80 classes)
            int candidates = tensor.Dimensions[2]; // Should be 8400

            var detections = new List<Detection>();
            float confidenceThreshold = 0.25f; // Minimum confidence to consider a detection
            float iouThreshold = 0.45f;        // Intersection over Union threshold for NMS

            // 2. Iterate over all 8400 candidates
            for (int i = 0; i < candidates; i++)
            {
                // Find the class with the highest score
                float maxScore = 0;
                int maxClass = -1;

                // Skip the first 4 coordinates (cx, cy, w, h)
                for (int c = 4; c < dimensions; c++)
                {
                    float score = tensor[0, c, i];
                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxClass = c - 4; // Shift index back to 0-based class ID
                    }
                }

                // Filter by confidence
                if (maxScore < confidenceThreshold) continue;

                // Extract coordinates (normalized 0-1 relative to 640x640)
                float cx = tensor[0, 0, i];
                float cy = tensor[0, 1, i];
                float w = tensor[0, 2, i];
                float h = tensor[0, 3, i];

                // Convert normalized center-x, center-y, width, height to absolute bounding box
                // Note: We are mapping back to the original image size here.
                // If you used letterboxing, you must undo the scaling and padding logic here.
                float x = (cx - w / 2) * originalWidth;
                float y = (cy - h / 2) * originalHeight;
                w *= originalWidth;
                h *= originalHeight;

                detections.Add(new Detection
                {
                    Box = new RectangleF(x, y, w, h),
                    ClassId = maxClass,
                    Score = maxScore
                });
            }

            // 3. Apply Non-Maximum Suppression (NMS)
            // This removes overlapping boxes for the same object.
            return ApplyNMS(detections, iouThreshold);
        }

        static List<Detection> ApplyNMS(List<Detection> detections, float iouThreshold)
        {
            // Sort by score descending
            var sorted = detections.OrderByDescending(d => d.Score).ToList();
            var result = new List<Detection>();

            while (sorted.Count > 0)
            {
                // Take the highest scoring detection
                var best = sorted[0];
                result.Add(best);
                sorted.RemoveAt(0);

                // Remove any remaining detections that overlap significantly with the best one
                sorted.RemoveAll(d => CalculateIoU(best.Box, d.Box) > iouThreshold);
            }

            return result;
        }

        static float CalculateIoU(RectangleF a, RectangleF b)
        {
            float areaA = a.Width * a.Height;
            float areaB = b.Width * b.Height;

            if (areaA == 0 || areaB == 0) return 0;

            float intersectionX = Math.Max(0, Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left));
            float intersectionY = Math.Max(0, Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top));
            float intersectionArea = intersectionX * intersectionY;

            float unionArea = areaA + areaB - intersectionArea;
            return unionArea == 0 ? 0 : intersectionArea / unionArea;
        }

        static void RenderBoxes(string inputPath, string outputPath, List<Detection> detections)
        {
            using Bitmap bitmap = new Bitmap(inputPath);
            using Graphics g = Graphics.FromImage(bitmap);
            
            // Load COCO class names (truncated for brevity)
            var classNames = new[] { "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light" }; // ... and 70 more

            using Pen pen = new Pen(Color.Red, 3);
            using Font font = new Font("Arial", 12);
            using SolidBrush brush = new SolidBrush(Color.Red);

            foreach (var det in detections)
            {
                // Draw bounding box
                g.DrawRectangle(pen, det.Box.X, det.Box.Y, det.Box.Width, det.Box.Height);

                // Draw label
                string label = classNames.Length > det.ClassId ? classNames[det.ClassId] : $"Class {det.ClassId}";
                string text = $"{label}: {det.Score:P0}";
                g.DrawString(text, font, brush, det.Box.X, det.Box.Y - 20);
            }

            bitmap.Save(outputPath, ImageFormat.Jpeg);
        }
    }

    // Data structure for a single detection
    public class Detection
    {
        public RectangleF Box { get; set; }
        public int ClassId { get; set; }
        public float Score { get; set; }
    }
}
