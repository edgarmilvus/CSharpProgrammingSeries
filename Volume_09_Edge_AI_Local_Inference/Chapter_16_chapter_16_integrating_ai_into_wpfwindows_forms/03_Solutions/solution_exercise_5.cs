
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // WPF specific
using Microsoft.ML.OnnxRuntime;

namespace ModelManagementApp
{
    // 1. Model Manager
    public class ModelManager : IDisposable
    {
        private readonly Dictionary<string, InferenceSession> _loadedModels = new Dictionary<string, InferenceSession>();
        private InferenceSession? _activeModel;
        private const long MemoryThreshold = 4L * 1024 * 1024 * 1024; // 4GB

        public string? ActiveModelName { get; private set; }

        // 2. Hot Swap Logic
        public async Task SwitchModel(string modelName, string filePath)
        {
            // 3. Memory Guard
            long currentMemory = GC.GetTotalMemory(true);
            if (currentMemory > MemoryThreshold)
            {
                // Force cleanup if memory is high
                Dispose(); 
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect(); // Final collection
            }

            // Unload current if exists
            if (_loadedModels.TryGetValue(modelName, out var existingModel))
            {
                _activeModel = existingModel;
                return; // Already loaded
            }

            // Load new model
            // Note: InferenceSession loading is synchronous and blocking.
            // We run it on a background thread to prevent UI freeze.
            await Task.Run(() =>
            {
                try
                {
                    var session = new InferenceSession(filePath);
                    _loadedModels[modelName] = session;
                    _activeModel = session;
                    ActiveModelName = modelName;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load model {modelName}", ex);
                }
            });
        }

        public InferenceSession? GetActiveSession() => _activeModel;

        public void Dispose()
        {
            foreach (var session in _loadedModels.Values)
            {
                session.Dispose();
            }
            _loadedModels.Clear();
            _activeModel = null;
        }
    }

    // UI Integration Example
    public partial class MainWindow : Window
    {
        private readonly ModelManager _modelManager = new ModelManager();
        private bool _isLoading = false;

        private async void OnModelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading || ModelComboBox.SelectedItem == null) return;

            var selectedModel = ModelComboBox.SelectedItem as ComboBoxItem;
            if (selectedModel == null) return;

            string modelName = selectedModel.Content.ToString();
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{modelName}.onnx");

            // 4. UI Feedback (Overlay)
            LoadingOverlay.Visibility = Visibility.Visible;
            _isLoading = true;

            try
            {
                await _modelManager.SwitchModel(modelName, modelPath);
                StatusText.Text = $"Active Model: {_modelManager.ActiveModelName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error switching model: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                _isLoading = false;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _modelManager.Dispose();
            base.OnClosed(e);
        }
    }
}
