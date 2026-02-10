
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

public class Trainer
{
    public void Train(List<TensorDataPoint> data)
    {
        float learningRate = 0.01f;

        // The lambda captures the 'learningRate' variable from the scope.
        // It doesn't evaluate 'learningRate' until the lambda is actually invoked.
        Func<TensorDataPoint, float> adjustStrategy = point => point.Value * learningRate;

        // Simulate an epoch where learning rate changes
        foreach (var point in data)
        {
            float adjustment = adjustStrategy(point);
            // Apply adjustment...
        }

        learningRate = 0.001f; // Decay learning rate

        // If we iterate again, the lambda uses the NEW learningRate value
        // because it captured the variable, not the value 0.01.
        foreach (var point in data)
        {
            float adjustment = adjustStrategy(point); 
            // adjustment now uses 0.001
        }
    }
}
