
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

// Source File: theory_theoretical_foundations_part4.cs
// Description: Theoretical Foundations
// ==========================================

public class UserInterface
{
    public void Initialize()
    {
        var model = new LargeLanguageModel();

        // SUBSCRIBING WITH A LAMBDA EXPRESSION
        // We define the handler logic right here.
        // 'sender' is the model that raised the event.
        // 'e' contains our Tensor data and metadata.
        model.ModelFinishedThinking += (sender, e) => 
        {
            if (e.IsSuccess)
            {
                Console.WriteLine($"UI Update: Model finished in {e.Metadata.Duration.TotalSeconds}s");
                Console.WriteLine($"Displaying Result: {e.ResultData}");
                
                // In a real GUI app, this would be:
                // Dispatcher.Invoke(() => { textBoxResult.Text = e.ResultData.ToString(); });
            }
            else
            {
                Console.WriteLine("UI Update: Model failed.");
            }
        };

        // Trigger the simulation
        model.StartInferenceAsync("What is AI?").Wait();
    }
}
