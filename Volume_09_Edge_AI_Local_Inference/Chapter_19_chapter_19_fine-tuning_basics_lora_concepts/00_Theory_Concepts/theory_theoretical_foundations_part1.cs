
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

// Conceptual representation of the LoRA forward pass logic
public interface IModelLayer
{
    Tensor Forward(Tensor input);
}

// The frozen, pre-trained base layer (High Rank)
public class FrozenBaseLayer : IModelLayer
{
    private readonly Tensor _baseWeights; // W0
    public Tensor Forward(Tensor input) => input.MatMul(_baseWeights);
}

// The LoRA Adapter (Low Rank)
public class LoraAdapter : IModelLayer
{
    private readonly IModelLayer _baseLayer;
    private readonly Tensor _adapterA; // Rank r
    private readonly Tensor _adapterB; // Rank r

    public LoraAdapter(IModelLayer baseLayer, Tensor adapterA, Tensor adapterB)
    {
        _baseLayer = baseLayer;
        _adapterA = adapterA;
        _adapterB = adapterB;
    }

    public Tensor Forward(Tensor input)
    {
        // h = xW0 + xBA
        var baseOutput = _baseLayer.Forward(input);
        var adapterOutput = input.MatMul(_adapterA).MatMul(_adapterB);
        return baseOutput + adapterOutput;
    }
}
