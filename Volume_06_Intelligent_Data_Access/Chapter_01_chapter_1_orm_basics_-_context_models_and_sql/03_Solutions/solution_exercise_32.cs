
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

# Source File: solution_exercise_32.cs
# Description: Solution for Exercise 32
# ==========================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

// 1. Locale-Aware Embedding Service
public class LocaleEmbeddingService
{
    private readonly Dictionary<string, IEmbeddingModel> _models;

    public LocaleEmbeddingService()
    {
        // We might have different models for different locales
        // Or a single model with locale hints
        _models = new Dictionary<string, IEmbeddingModel>();
    }

    public async Task<float[]> GenerateAsync(string text, string locale)
    {
        // 1. Cultural Normalization
        // Normalize text based on culture (e.g., date formats, currency)
        var normalizedText = NormalizeText(text, locale);

        // 2. Locale-Specific Model
        // If we have a model specifically trained on French legal text, use it for "fr-FR"
        if (_models.TryGetValue(locale, out var model))
        {
            return await model.GenerateAsync(normalizedText);
        }

        // Fallback to generic multilingual model with locale hint
        return await _models["generic"].GenerateAsync(normalizedText + $" [Locale: {locale}]");
    }

    private string NormalizeText(string text, string locale)
    {
        var culture = new CultureInfo(locale);
        // Example: Normalize casing specific to the language
        return text.ToUpper(culture);
    }
}

// 2. Localized Similarity Metric
public class LocalizedSimilarity
{
    public double Calculate(float[] a, float[] b, string locale)
    {
        // In some cultures, specific terms might be weighted differently.
        // However, vector math is universal.
        // The localization happens in the Embedding generation, not the similarity metric.
        
        // Standard Cosine Similarity
        return DotProduct(a, b) / (Magnitude(a) * Magnitude(b));
    }

    private double DotProduct(float[] a, float[] b) => 0.9; // Mock
    private double Magnitude(float[] a) => 1.0; // Mock
}
