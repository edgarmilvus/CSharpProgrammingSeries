
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

// Source File: theory_theoretical_foundations_part2.cs
// Description: Theoretical Foundations
// ==========================================

// The entry point of the containerized agent.
// This class represents the "Chassis" of our truck.
using Microsoft.Extensions.Hosting;

namespace CloudNativeAI.Agents.Runtime
{
    public class InferenceHostedService : IHostedService
    {
        private readonly IModelLoader<InferenceEngine> _modelLoader;
        
        public InferenceHostedService(IModelLoader<InferenceEngine> modelLoader)
        {
            _modelLoader = modelLoader;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // On startup, the container attaches the "Trailer" (Volume)
            // and loads the weights into GPU memory.
            var metadata = new ModelMetadata 
            { 
                StoragePath = "/mnt/models/llama-3-70b" // Kubernetes Volume Mount Path
            };
            await _modelLoader.LoadAsync(metadata);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
