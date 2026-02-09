
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

// Example: High-Speed Image Processing using Unsafe Pointers
// Context: In AI workloads (e.g., computer vision), we often need to apply
// simple transformations (like increasing brightness) to large image buffers.
// Using safe C# code with array indexing involves bounds checking on every access,
// which can be a bottleneck. Using 'unsafe' pointers allows direct memory access,
// bypassing these checks for maximum performance.

using System;
using System.Drawing; // Requires reference to System.Drawing.Common NuGet package
using System.Drawing.Imaging;

namespace HighPerformanceAI
{
    public class ImageProcessor
    {
        // The 'unsafe' keyword allows us to define a method that contains pointer syntax.
        // The runtime will allow code that modifies the IL (Intermediate Language) to 
        // perform unverifiable operations.
        public static unsafe void IncreaseBrightnessUnsafe(Bitmap image, byte brightnessIncrement)
        {
            // 1. Lock the bitmap in memory.
            // We use LockBits to get a pointer to the first pixel.
            // This is crucial because the Garbage Collector (GC) might move the bitmap
            // data in memory while we are working with it. LockBits ensures the memory address is fixed.
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

            // 2. Get the pointer to the first pixel.
            // bmpData.Scan0 returns an IntPtr (a raw memory address).
            // We cast this IntPtr to a byte* (pointer to byte).
            // We assume PixelFormat.Format24bppRgb (3 bytes per pixel: Blue, Green, Red).
            byte* ptr = (byte*)bmpData.Scan0.ToPointer();

            // 3. Calculate total bytes.
            // Stride is the width of a single row of pixels in bytes, including padding.
            // It might be wider than the actual image width (Width * 3).
            int bytesPerPixel = 3;
            int height = image.Height;
            int width = image.Width;
            int stride = bmpData.Stride; // The distance (in bytes) to the next row
            int totalBytes = Math.Abs(stride) * height;

            // 4. Iterate through the memory directly.
            // We are iterating byte-by-byte. This is significantly faster than
            // image.GetPixel(x, y) and image.SetPixel(x, y) in a nested loop.
            for (int i = 0; i < totalBytes; i++)
            {
                // 'ptr[i]' is equivalent to '*(ptr + i)'.
                // We are reading the byte value at the current memory address.
                int newValue = ptr[i] + brightnessIncrement;

                // 5. Clamp the value.
                // Pixel values are 0-255. We must prevent overflow (wrapping around).
                // If the value exceeds 255, we cap it at 255.
                if (newValue > 255) newValue = 255;
                
                // 6. Write the value back.
                ptr[i] = (byte)newValue;
            }

            // 7. Unlock the bitmap.
            // This tells the OS that we are done manipulating the memory directly.
            // If we forget this, the application might crash or the bitmap will remain locked.
            image.UnlockBits(bmpData);
        }

        public static void Main()
        {
            // Create a dummy image for demonstration (100x100 pixels)
            using (Bitmap bmp = new Bitmap(100, 100))
            {
                // Fill with a dark gray color
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        bmp.SetPixel(x, y, Color.FromArgb(50, 50, 50));
                    }
                }

                Console.WriteLine("Processing image with unsafe pointers...");
                
                // Call the unsafe method
                // Note: To run this, you must allow unsafe code in your project settings:
                // <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                IncreaseBrightnessUnsafe(bmp, 100);

                // Verify a pixel
                Color pixel = bmp.GetPixel(50, 50);
                Console.WriteLine($"Pixel at (50,50) is now: R={pixel.R}, G={pixel.G}, B={pixel.B}");
                // Expected: R=150, G=150, B=150 (50 + 100)
            }
        }
    }
}
