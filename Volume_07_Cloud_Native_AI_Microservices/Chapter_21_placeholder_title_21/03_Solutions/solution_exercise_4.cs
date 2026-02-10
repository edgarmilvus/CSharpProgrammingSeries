
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

// Controllers/InferenceController.cs
using InferenceService.Services;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json;

namespace InferenceService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InferenceController : ControllerBase
{
    private readonly BatchingService _batchingService;
    private readonly ILogger<InferenceController> _logger;

    public InferenceController(BatchingService batchingService, ILogger<InferenceController> logger)
    {
        _batchingService = batchingService;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<IActionResult> Infer([FromForm] IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("No image uploaded.");

        using var stream = image.OpenReadStream();
        using var img = await Image.LoadAsync<Rgb24>(stream);
        
        // Resize to 224x224
        img.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(224, 224),
            Mode = ResizeMode.Crop
        }));

        // Convert to Tensor
        var tensor = ConvertImageToTensor(img);

        // Submit to batching service
        var tcs = new TaskCompletionSource<float[]>();
        await _batchingService.SubmitAsync(tensor, tcs);

        var result = await tcs.Task;
        
        return Ok(new { prediction = result });
    }

    private static DenseTensor<float> ConvertImageToTensor(Image<Rgb24> image)
    {
        // HWC to CHW format for ONNX models usually required
        var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
        
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                // Normalize to [0, 1] if model expects it
                tensor[0, 0, y, x] = pixel.R / 255.0f;
                tensor[0, 1, y, x] = pixel.G / 255.0f;
                tensor[0, 2, y, x] = pixel.B / 255.0f;
            }
        }
        return tensor;
    }
}
