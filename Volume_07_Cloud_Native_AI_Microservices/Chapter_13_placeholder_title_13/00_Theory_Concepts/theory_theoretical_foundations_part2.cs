
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

using System.Collections.Generic;
using System.Threading.Tasks;

// The abstraction for memory, decoupling the agent from the storage implementation.
public interface IMemoryStore
{
    Task<string> RetrieveAsync(string query);
    Task StoreAsync(string key, string value);
}

// The agent depends on the abstraction, not the concrete class.
public class StatefulAgent : IAgent
{
    private readonly IMemoryStore _memory;
    private readonly IModelClient _modelClient;

    public StatefulAgent(IModelClient modelClient, IMemoryStore memory)
    {
        _modelClient = modelClient;
        _memory = memory;
    }

    public string Id => "Stateful-Agent-v1";

    public async Task<AgentResponse> RespondAsync(ChatMessage[] context)
    {
        // 1. Retrieve relevant context from long-term memory
        var historicalContext = await _memory.RetrieveAsync(context[0].Text);

        // 2. Augment the prompt with this context
        var augmentedPrompt = $"{historicalContext}\nUser: {context[0].Text}";

        // 3. Generate response
        var response = await _modelClient.CompleteAsync(new[] { new ChatMessage { Role = "User", Content = augmentedPrompt } });

        // 4. Store new information (if applicable)
        await _memory.StoreAsync(context[0].Text, response.Content);

        return response;
    }
}
