
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using System.Threading.Tasks;
using MassTransit;

namespace AgentSwarm.Agents.Consumers
{
    // This class represents an agent that listens for tasks on a message queue.
    public class InferenceAgent : IConsumer<InferenceTask>
    {
        private readonly IInferenceEngine _inferenceEngine;

        // The IInferenceEngine is injected, demonstrating the power of the interface defined earlier.
        public InferenceAgent(IInferenceEngine inferenceEngine)
        {
            _inferenceEngine = inferenceEngine;
        }

        public async Task Consume(ConsumeContext<InferenceTask> context)
        {
            var task = context.Message;
            
            // The agent's core logic: process the task using the injected engine.
            var result = await _inferenceEngine.InferAsync(task.Prompt, task.Context);

            // Publish a result event for other parts of the system to consume.
            await context.Publish(new InferenceResult { TaskId = task.Id, Result = result });
        }
    }

    // A simple message contract for the task queue.
    public record InferenceTask
    {
        public Guid Id { get; init; }
        public string Prompt { get; init; }
        public InferenceContext Context { get; init; }
    }

    public record InferenceResult
    {
        public Guid TaskId { get; init; }
        public string Result { get; init; }
    }
}
