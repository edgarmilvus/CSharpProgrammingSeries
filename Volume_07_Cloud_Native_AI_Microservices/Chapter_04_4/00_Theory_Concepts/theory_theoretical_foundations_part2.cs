
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using Microsoft.FeatureManagement;

public class InferenceService
{
    private readonly IFeatureManager _featureManager;
    private readonly IModelRunner _v1Runner;
    private readonly IModelRunner _v2Runner;

    public InferenceService(IFeatureManager featureManager, 
                            V1ModelRunner v1Runner, 
                            V2ModelRunner v2Runner)
    {
        _featureManager = featureManager;
        _v1Runner = v1Runner;
        _v2Runner = v2Runner;
    }

    public async Task<InferenceResult> PredictAsync(InputData input)
    {
        // Check if the new model is enabled for this request (e.g., based on user ID or random percentage)
        if (await _featureManager.IsEnabledAsync("V2ModelEnabled"))
        {
            return await _v2Runner.ExecuteAsync(input);
        }
        
        return await _v1Runner.ExecuteAsync(input);
    }
}
