
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

// Source File: theory_theoretical_foundations_part5.cs
// Description: Theoretical Foundations
// ==========================================

using System;

public class TemperatureControlledProcessor
{
    public void Process()
    {
        // This variable is local to the Process method
        double temperature = 0.7; 

        // We define a lambda that "captures" the 'temperature' variable.
        // Even though 'GenerateResponse' is passed around, it remembers the value of 'temperature'.
        Func<string, string> GenerateResponse = (prompt) => 
        {
            // Simulate AI generation with the captured temperature
            return $"[Temp: {temperature}] AI Response to: {prompt}";
        };

        // Simulate sending this function to a tensor processing unit
        SendToTensorPipeline(GenerateResponse);
    }

    private void SendToTensorPipeline(Func<string, string> processor)
    {
        // The processor is executed here, but it still knows about 'temperature' from Process()
        string result = processor("What is AI?");
        Console.WriteLine(result);
    }
}
