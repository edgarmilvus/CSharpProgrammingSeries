
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public record BoundingBox(float X, float Y, float Width, float Height);
public record DetectionResult(BoundingBox Box, int ClassId, float Score);

public class NmsProcessor
{
    // YOLOv8 output shape: [1, 84, 8400] -> 84 = 4 coords + 80 classes
    public List<DetectionResult> Process(Tensor<float> outputTensor, float confidenceThreshold = 0.5f, float iouThreshold = 0.45f)
    {
        var results = new List<DetectionResult>();
        
        // Dimensions
        int dimensions = outputTensor.Dimensions[1]; // 84
        int boxesCount = outputTensor.Dimensions[2]; // 8400

        // Extract data for efficient access
        var data = outputTensor.ToArray(); // Flatten for linear access

        // Temporary list to hold all potential detections
        var candidates = new List<(int Index, float MaxScore, int ClassId, BoundingBox Box)>();

        for (int i = 0; i < boxesCount; i++)
        {
            // Find max class score and its index
            float maxScore = 0;
            int classId = -1;
            
            // Skip first 4 coordinates (0-3), scores start at index 4
            for (int c = 0; c < 80; c++)
            {
                float score = data[i + (4 + c) * boxesCount + 1]; // Manual stride calculation if flat
                // Note: Accessing Tensor<float> via indexer is safer for multi-dim:
                // outputTensor[0, 4 + c, i]
                
                // Correct flat access assuming data is row-major (standard ML.NET tensors)
                // [1, 84, 8400] -> index = (0 * 84 * 8400) + (c * 8400) + i
                float s = outputTensor[0, 4 + c, i]; 
                
                if (s > maxScore)
                {
                    maxScore = s;
                    classId = c;
                }
            }

            if (maxScore >= confidenceThreshold)
            {
                // Extract Box (cx, cy, w, h)
                float cx = outputTensor[0, 0, i];
                float cy = outputTensor[0, 1, i];
                float w = outputTensor[0, 2, i];
                float h = outputTensor[0, 3, i];

                candidates.Add((i, maxScore, classId, new BoundingBox(cx, cy, w, h)));
            }
        }

        // Sort by score descending
        candidates = candidates.OrderByDescending(x => x.MaxScore).ToList();

        // Perform NMS
        var activeCandidates = new List<(int Index, float MaxScore, int ClassId, BoundingBox Box)>(candidates);
        
        while (activeCandidates.Count > 0)
        {
            // Pick highest score
            var detection = activeCandidates[0];
            activeCandidates.RemoveAt(0);

            // Convert center format (cx, cy, w, h) to top-left/bottom-right (x1, y1, x2, y2) for IoU
            var boxA = ConvertToXYXY(detection.Box);
            
            // Add to final results
            results.Add(new DetectionResult(detection.Box, detection.ClassId, detection.MaxScore));

            // Filter out overlapping boxes
            activeCandidates.RemoveAll(other =>
            {
                var boxB = ConvertToXYXY(other.Box);
                return CalculateIoU(boxA, boxB) > iouThreshold && other.ClassId == detection.ClassId;
            });
        }

        return results;
    }

    private (float X1, float Y1, float X2, float Y2) ConvertToXYXY(BoundingBox box)
    {
        float x1 = box.X - box.Width / 2;
        float y1 = box.Y - box.Height / 2;
        float x2 = box.X + box.Width / 2;
        float y2 = box.Y + box.Height / 2;
        return (x1, y1, x2, y2);
    }

    public float CalculateIoU((float X1, float Y1, float X2, float Y2) a, (float X1, float Y1, float X2, float Y2) b)
    {
        float intersectionX = Math.Max(0, Math.Min(a.X2, b.X2) - Math.Max(a.X1, b.X1));
        float intersectionY = Math.Max(0, Math.Min(a.Y2, b.Y2) - Math.Max(a.Y1, b.Y1));
        float intersectionArea = intersectionX * intersectionY;

        float areaA = (a.X2 - a.X1) * (a.Y2 - a.Y1);
        float areaB = (b.X2 - b.X1) * (b.Y2 - b.Y1);
        
        float unionArea = areaA + areaB - intersectionArea;

        return unionArea == 0 ? 0 : intersectionArea / unionArea;
    }
}
