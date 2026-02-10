
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

// File: Configuration/BaseServiceOptions.cs
namespace Configuration;

public abstract class BaseServiceOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}

// File: Configuration/TextGenerationOptions.cs
namespace Configuration;

public class TextGenerationOptions : BaseServiceOptions
{
    // Specific properties for text generation can go here
}

// File: Configuration/ImageProcessingOptions.cs
namespace Configuration;

public class ImageProcessingOptions : BaseServiceOptions
{
    // Specific properties for image processing can go here
}

// File: Services/MultiModelOrchestrator.cs
using Configuration;
using Microsoft.Extensions.Options;

namespace Services;

public class MultiModelOrchestrator
{
    private readonly IOptions<TextGenerationOptions> _textOptions;
    private readonly IOptions<ImageProcessingOptions> _imageOptions;

    public MultiModelOrchestrator(
        IOptions<TextGenerationOptions> textOptions, 
        IOptions<ImageProcessingOptions> imageOptions)
    {
        _textOptions = textOptions;
        _imageOptions = imageOptions;
    }

    public List<(string Name, string Endpoint)> GetActiveServices()
    {
        var activeServices = new List<(string, string)>();

        // Check Text Generation
        var textConfig = _textOptions.Value;
        if (!string.IsNullOrWhiteSpace(textConfig.ApiKey) && !string.IsNullOrWhiteSpace(textConfig.Endpoint))
        {
            activeServices.Add(("Text Generation", textConfig.Endpoint));
        }

        // Check Image Processing
        var imageConfig = _imageOptions.Value;
        if (!string.IsNullOrWhiteSpace(imageConfig.ApiKey) && !string.IsNullOrWhiteSpace(imageConfig.Endpoint))
        {
            activeServices.Add(("Image Processing", imageConfig.Endpoint));
        }

        return activeServices;
    }
}

// File: Program.cs (Relevant sections)
using Configuration;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Register options for both services
builder.Services.Configure<TextGenerationOptions>(
    builder.Configuration.GetSection("ExternalServices:TextGeneration"));
    
builder.Services.Configure<ImageProcessingOptions>(
    builder.Configuration.GetSection("ExternalServices:ImageProcessing"));

builder.Services.AddScoped<MultiModelOrchestrator>();
