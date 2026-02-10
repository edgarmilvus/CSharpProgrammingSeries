
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

// Source File: theory_theoretical_foundations_part6.cs
// Description: Theoretical Foundations
// ==========================================

public interface IModel<in TInput, out TOutput> where TInput : ITensor<float> where TOutput : ITensor<float>
{
    TOutput Predict(TInput input);
}

// OpenAI model (simulated)
public class OpenAiModel : IModel<ITensor<float>, ITensor<float>>
{
    public ITensor<float> Predict(ITensor<float> input)
    {
        // Call OpenAI API
        return new Tensor<float, Shape1D>(new Shape1D { Size = 10 }); // Example output
    }
}

// Local Llama model
public class LlamaModel : IModel<ITensor<float>, ITensor<float>>
{
    public ITensor<float> Predict(ITensor<float> input)
    {
        // Run local inference
        return new Tensor<float, Shape1D>(new Shape1D { Size = 10 });
    }
}

// Usage in pipeline
public class InferencePipeline<TModel, TInput, TOutput> 
    where TModel : IModel<TInput, TOutput> 
    where TInput : ITensor<float> 
    where TOutput : ITensor<float>
{
    private TModel model;
    public InferencePipeline(TModel model) { this.model = model; }

    public TOutput Run(TInput input)
    {
        return model.Predict(input);
    }
}
