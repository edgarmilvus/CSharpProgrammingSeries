
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

// Source File: theory_theoretical_foundations_part3.cs
// Description: Theoretical Foundations
// ==========================================

using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class InferenceWorker : BackgroundService
{
    private readonly IInferenceEngine _engine;
    private readonly IMessageQueue _queue;

    public InferenceWorker(IInferenceEngine engine, IMessageQueue queue)
    {
        _engine = engine;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Continuously poll the queue for inference requests
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await _queue.ReceiveAsync(stoppingToken);
            if (message != null)
            {
                var result = await _engine.GenerateAsync(message.Payload);
                await _queue.PublishResultAsync(message.Id, result);
            }
        }
    }
}
