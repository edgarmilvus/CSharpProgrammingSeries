
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AiAgent
{
    private AgentState _state;
    private readonly DistributedStateStore<AgentState> _stateStore;
    private readonly ILogger<AiAgent> _logger;
    private readonly SemaphoreSlim _stateLock = new SemaphoreSlim(1, 1);
    private readonly string _agentId;

    public AiAgent(string agentId, DistributedStateStore<AgentState> stateStore, ILogger<AiAgent> logger)
    {
        _agentId = agentId;
        _stateStore = stateStore;
        _logger = logger;
        _state = new AgentState(); // Default initialization
    }

    public async Task InitializeAsync()
    {
        // Use the lock to ensure thread-safe initialization
        await _stateLock.WaitAsync();
        try
        {
            _logger.LogInformation("Initializing agent state from Redis...");
            try
            {
                var loadedState = await _stateStore.GetStateAsync(_agentId);
                if (loadedState != null)
                {
                    _state = loadedState;
                    _logger.LogInformation("State loaded successfully. Message count: {Count}", _state.MessageCount);
                }
                else
                {
                    _logger.LogInformation("No existing state found. Starting with default state.");
                }
            }
            catch (StateNotFoundException)
            {
                _logger.LogInformation("State not found in Redis. Using default state.");
            }
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        await _stateLock.WaitAsync();
        try
        {
            // 1. Update In-Memory State
            _state.MessageCount++;
            _state.History.Add(message);
            
            // 2. Persist to Redis
            // We do this inside the lock to ensure we don't persist a state 
            // that is currently being modified by another thread.
            try
            {
                await _stateStore.SetStateAsync(_agentId, _state, TimeSpan.FromHours(1));
            }
            catch (RedisConnectionException ex)
            {
                // Edge Case: Redis Failure
                // Log the error and emit a metric/event (simulated here by logging)
                _logger.LogError(ex, "Failed to persist state to Redis. State drift detected.");
                
                // In a real scenario, you might push to a dead-letter queue or 
                // increment a "StateDrift" metric counter here.
            }

            // 3. Generate Response
            return $"Processed message #{_state.MessageCount}: {message}";
        }
        finally
        {
            _stateLock.Release();
        }
    }
}

public class AgentState
{
    public int MessageCount { get; set; }
    public List<string> History { get; set; } = new();
}
