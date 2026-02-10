
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Runtime.InteropServices;

public class ImagePreprocessor
{
    public Tensor<float> PreprocessImage(Image<Rgba32> image, int targetWidth, int targetHeight)
    {
        // 1. Calculate Letterbox dimensions
        float aspectRatio = Math.Min((float)targetWidth / image.Width, (float)targetHeight / image.Height);
        int newWidth = (int)(image.Width * aspectRatio);
        int newHeight = (int)(image.Height * aspectRatio);

        // 2. Resize using ImageSharp (optimized native code)
        // We clone to avoid modifying the original source frame
        using var clonedImage = image.Clone(ctx => ctx.Resize(newWidth, newHeight));

        // 3. Create target tensor [1, 3, 640, 640]
        var tensor = new DenseTensor<float>(new[] { 1, 3, targetWidth, targetHeight });
        
        // 4. Calculate padding
        int padX = (targetWidth - newWidth) / 2;
        int padY = (targetHeight - newHeight) / 2;

        // 5. Efficient Pixel Access using Span
        // We need to normalize 0-255 to 0-1.0f
        // ImageSharp stores pixels as Rgba32 struct (4 bytes)
        var pixelSpan = clonedImage.GetPixelSpan();
        
        // Since we need to transpose HWC to CHW, we iterate carefully
        // We map the resized image area into the center of the tensor
        for (int y = 0; y < newHeight; y++)
        {
            // Get row span for the current y
            Span<Rgba32> rowSpan = clonedImage.GetPixelRowSpan(y);
            
            for (int x = 0; x < newWidth; x++)
            {
                Rgba32 pixel = rowSpan[x];
                
                // Target coordinates in the tensor (CHW layout)
                // Channel 0 (R)
                tensor[0, 0, padY + y, padX + x] = pixel.R / 255.0f;
                // Channel 1 (G)
                tensor[0, 1, padY + y, padX + x] = pixel.G / 255.0f;
                // Channel 2 (B)
                tensor[0, 2, padY + y, padX + x] = pixel.B / 255.0f;
            }
        }

        return tensor;
    }
}
