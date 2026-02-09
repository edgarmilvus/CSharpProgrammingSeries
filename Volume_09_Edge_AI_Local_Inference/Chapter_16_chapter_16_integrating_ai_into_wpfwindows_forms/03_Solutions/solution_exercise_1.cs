
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfInferenceApp
{
    public class LocalInferenceService
    {
        private InferenceSession? _session;
        private readonly string _modelPath;

        public LocalInferenceService(string modelPath)
        {
            _modelPath = modelPath;
        }

        private void InitializeSession()
        {
            if (_session != null) return;

            var options = new SessionOptions();
            
            // Attempt to use CUDA if available, otherwise CPU
            try
            {
                // 0 is usually the default CUDA device ID
                options.AppendExecutionProvider_CUDA(0); 
            }
            catch (Exception)
            {
                // Fallback to CPU if CUDA provider fails to initialize
                options.AppendExecutionProvider_CPU();
            }

            _session = new InferenceSession(_modelPath, options);
        }

        public async IAsyncEnumerable<string> GenerateAsync(string prompt, CancellationToken token)
        {
            // Ensure session is created (lazy initialization)
            if (_session == null) InitializeSession();

            // NOTE: This is a simplified simulation of an LLM generation loop 
            // since we don't have a specific tokenizer/model structure defined in the prompt.
            // In a real scenario, you would tokenize the prompt here.
            
            // Simulating the generation of tokens asynchronously
            // We yield strings to the caller (UI) to update the view
            var simulatedTokens = new[] { "The", " quick", " brown", " fox", " jumps", " over", " the", " lazy", " dog." };

            foreach (var tokenChunk in simulatedTokens)
            {
                token.ThrowIfCancellationRequested();
                
                // Simulate network/computation delay
                await Task.Delay(100, token); 
                
                yield return tokenChunk;
            }
        }

        public void DisposeSession()
        {
            _session?.Dispose();
        }
    }

    // MainWindow.xaml.cs Context
    public partial class MainWindow : Window
    {
        private readonly LocalInferenceService _inferenceService;
        private CancellationTokenSource? _cts;

        public MainWindow()
        {
            InitializeComponent();
            // Path to your .onnx model file
            _inferenceService = new LocalInferenceService("path/to/model.onnx");
        }

        private async void OnGenerateClicked(object sender, RoutedEventArgs e)
        {
            // Reset UI
            OutputText.Text = string.Empty;
            GenerationProgressBar.IsIndeterminate = true; // Activate indeterminate style
            _cts = new CancellationTokenSource();

            try
            {
                await foreach (var token in _inferenceService.GenerateAsync("User Prompt", _cts.Token))
                {
                    OutputText.Text += token;
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Generation cancelled.");
            }
            catch (OnnxRuntimeException ex)
            {
                MessageBox.Show($"ONNX Error: {ex.Message}", "Inference Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"General Error: {ex.Message}");
            }
            finally
            {
                GenerationProgressBar.IsIndeterminate = false;
            }
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        protected override void OnClosed(EventArgs e)
        {
            _inferenceService.DisposeSession();
            base.OnClosed(e);
        }
    }
}
