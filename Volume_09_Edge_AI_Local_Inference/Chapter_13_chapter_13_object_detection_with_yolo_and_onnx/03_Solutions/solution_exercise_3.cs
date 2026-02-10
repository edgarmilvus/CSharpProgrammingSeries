
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System.Collections.Concurrent; // For BlockingCollection if preferred, but Channels is modern

// Mock Video Capture Interface (Abstracting AForge/OpenCvSharp)
public interface IVideoCapture
{
    Image<Rgba32> CaptureFrame();
}

public class RealTimeDetector
{
    private readonly Channel<Image<Rgba32>> _frameChannel;
    private readonly IVideoCapture _capture;
    private readonly NmsProcessor _nmsProcessor;
    private readonly ImagePreprocessor _preprocessor;
    private readonly InferenceSession _session; // Assumed initialized elsewhere
    
    // Event for UI to subscribe to
    public event Action<Image<Rgba32>, System.Collections.Generic.List<DetectionResult>> OnResultsReady;

    public RealTimeDetector(IVideoCapture capture)
    {
        _capture = capture;
        _frameChannel = Channel.CreateBounded<Image<Rgba32>>(new BoundedChannelOptions(1) 
            { FullMode = BoundedChannelFullMode.DropOldest }); // Drop frames if UI is slow
        _nmsProcessor = new NmsProcessor();
        _preprocessor = new ImagePreprocessor();
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        // Producer Task (Captures frames)
        var producer = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var frame = _capture.CaptureFrame();
                if (frame != null)
                {
                    await _frameChannel.Writer.WriteAsync(frame, cancellationToken);
                }
                // Small delay to simulate 30 FPS or match camera rate
                await Task.Delay(33, cancellationToken); 
            }
        }, cancellationToken);

        // Consumer Task (Inference & Post-processing)
        var consumer = Task.Run(async () =>
        {
            await foreach (var frame in _frameChannel.Reader.ReadAllAsync(cancellationToken))
            {
                // 1. Preprocess
                var inputTensor = _preprocessor.PreprocessImage(frame, 640, 640);
                
                // 2. Inference
                using var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("images", inputTensor)
                };
                
                // Run on background thread
                using var results = _session.Run(inputs);
                var outputTensor = results.First().AsTensor<float>();
                
                // 3. Post-process
                var detections = _nmsProcessor.Process(outputTensor);

                // 4. Notify UI (Must be marshalled to UI thread in UI implementation)
                OnResultsReady?.Invoke(frame, detections);
                
                // Dispose frame to prevent memory leak
                frame.Dispose();
                inputTensor.Dispose();
            }
        }, cancellationToken);

        await Task.WhenAll(producer, consumer);
    }
}

// Example SkiaSharp Rendering Logic (To be called on UI Thread)
public class DetectionCanvas : SKCanvasView
{
    private Image<Rgba32> _currentFrame;
    private System.Collections.Generic.List<DetectionResult> _currentDetections;
    private readonly object _updateLock = new object();

    // Call this from the background detector's event
    public void UpdateFrame(Image<Rgba32> frame, System.Collections.Generic.List<DetectionResult> detections)
    {
        lock (_updateLock)
        {
            // Dispose old frame if exists
            _currentFrame?.Dispose();
            _currentFrame = frame; // Take ownership
            _currentDetections = detections;
            
            // Request repaint on UI Thread
            InvalidateSurface();
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        lock (_updateLock)
        {
            if (_currentFrame == null) return;

            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            // Convert ImageSharp Image to SKBitmap efficiently
            // Note: ImageSharp uses Rgba32, Skia uses Rgba8888 (same layout usually)
            // We can pin memory to avoid copy if formats match perfectly, otherwise a copy is safer.
            using var bitmap = new SKBitmap(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            
            // Copy data (Optimization: Use unsafe code or MemoryMarshal.Cast for zero-copy if alignment matches)
            _currentFrame.CopyPixelDataTo(bitmap.Bytes); 
            
            canvas.DrawBitmap(bitmap, info.Rect);

            // Draw Detections
            using var paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
            using var textPaint = new SKPaint { Color = SKColors.Yellow, TextSize = 24 };

            foreach (var det in _currentDetections)
            {
                // Map normalized coordinates back to screen (assuming 640x640 input mapped to canvas size)
                // In real app, you need aspect ratio mapping logic here
                var rect = new SKRect(
                    det.Box.X - det.Box.Width / 2, 
                    det.Box.Y - det.Box.Height / 2,
                    det.Box.X + det.Box.Width / 2,
                    det.Box.Y + det.Box.Height / 2
                );
                
                canvas.DrawRect(rect, paint);
                canvas.DrawText($"{det.ClassId}: {det.Score:F2}", rect.Left, rect.Top - 5, textPaint);
            }
        }
    }
}
