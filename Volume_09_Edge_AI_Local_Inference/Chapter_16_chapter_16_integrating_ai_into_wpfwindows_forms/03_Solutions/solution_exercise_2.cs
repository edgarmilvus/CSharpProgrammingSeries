
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ML.OnnxRuntime; // Assuming usage for context

namespace WinFormsLogViewer
{
    public partial class MainForm : Form
    {
        private readonly SynchronizationContext _syncContext;
        private readonly List<string> _tokenBuffer = new List<string>();
        private const int BufferSize = 10; // Update UI every 10 tokens

        public MainForm()
        {
            InitializeComponent();
            // Capture the UI thread's synchronization context
            _syncContext = SynchronizationContext.Current;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            rtbOutput.Clear();

            // Simulate loading a model (Memory Management)
            using (var session = new InferenceSession("model.onnx")) 
            {
                try
                {
                    await RunInferenceAsync(session);
                    // Flush any remaining tokens in buffer
                    FlushBuffer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            } // 'session' is disposed here automatically via 'using'

            btnStart.Enabled = true;
        }

        private async Task RunInferenceAsync(InferenceSession session)
        {
            // Simulate processing a large file/stream
            // In reality, this would be a while loop reading logits
            for (int i = 1; i <= 100; i++)
            {
                // Simulate token generation
                string token = $"Token_{i} ";
                _tokenBuffer.Add(token);

                // Performance Optimization: Batch UI updates
                if (_tokenBuffer.Count >= BufferSize)
                {
                    FlushBuffer();
                }

                // Simulate work delay
                await Task.Delay(50);
            }
        }

        private void FlushBuffer()
        {
            if (_tokenBuffer.Count == 0) return;

            string textToAppend = string.Join("", _tokenBuffer);
            _tokenBuffer.Clear();

            // Thread-safe UI update using SynchronizationContext
            _syncContext.Post(_ => 
            {
                // This runs on the UI thread
                rtbOutput.AppendText(textToAppend);
                rtbOutput.ScrollToCaret(); // Keep view scrolled to bottom
            }, null);
        }
    }
}
