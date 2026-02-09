
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System.Text.Json;
using System.Threading.Channels;
using McpDotNet.Client;
using McpDotNet.Configuration;
using McpDotNet.Protocol.Transport;
using McpDotNet.Protocol.Messages;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// 1. Define the Data Transfer Objects (DTOs) for our tools.
// In a microservices environment, these contracts are shared via NuGet packages
// or interface definitions to ensure type safety across service boundaries.
public record OrderStatusRequest(string OrderId);
public record OrderStatusResponse(string OrderId, string Status, string EstimatedDelivery);
public record KnowledgeBaseRequest(string Query);
public record KnowledgeBaseResponse(string Answer);

public class Program
{
    public static async Task Main(string[] args)
    {
        // 2. Setup Dependency Injection and Logging (Standard .NET Host pattern)
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // 3. Configure the LLM (Large Language Model) Client.
        // We use the IChatClient abstraction from Microsoft.Extensions.AI.
        // This allows swapping providers (OpenAI, Azure, Local) without changing agent logic.
        // For this demo, we use a simple Echo client to simulate an LLM without needing API keys.
        services.AddSingleton<IChatClient, DemoEchoChatClient>();

        // 4. Configure the MCP (Model Context Protocol) Client.
        // MCP is the USB-C port for AI applications. It allows agents to connect to 
        // external tools (servers) dynamically.
        services.AddMcpClient(options =>
        {
            options.Id = "demo-agent-client";
            // In a real scenario, this would point to a deployed microservice URL.
            // We will launch a local mock server in the next step.
            options.ServerEndpoint = new Uri("http://localhost:5000");
            options.TransportType = TransportType.ServerSentEvents; 
        });

        var serviceProvider = services.BuildServiceProvider();

        // 5. Initialize the MCP Client and Connect to the Tool Server.
        var mcpClient = serviceProvider.GetRequiredService<IMcpClient>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try 
        {
            // Establishes the connection and discovers available tools.
            await mcpClient.ConnectAsync(); 
            logger.LogWarning("Connected to MCP Tool Server.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to connect to MCP Server: {ex.Message}");
            logger.LogWarning("Ensure the mock server is running on port 5000.");
            return; 
        }

        // 6. Bridge MCP Tools to the LLM Interface.
        // The IChatClient expects tools in a specific format. We adapt the 
        // MCP-discovered tools so the LLM can see and call them.
        var chatClient = serviceProvider.GetRequiredService<IChatClient>();
        var tools = new List<AIFunction>();
        
        foreach (var tool in mcpClient.Tools)
        {
            // Capture the tool reference for the closure
            var currentTool = tool; 
            tools.Add(AIFunctionFactory.Create(
                async (object? args) => 
                {
                    // Dynamically invoke the tool on the remote MCP server
                    var result = await mcpClient.CallToolAsync(currentTool.Name, args);
                    return result.Content; // Return the raw content back to the LLM
                },
                currentTool.Name,
                currentTool.Description
            ));
        }

        // 7. Define the User Query
        // The agent needs to decide: Is this a general question (KB) or specific order lookup (API)?
        var userPrompt = "What is the status of order #67890?";

        // 8. Execute the Agent Loop
        Console.WriteLine($"[User]: {userPrompt}");
        
        var chatOptions = new ChatOptions
        {
            Tools = tools, // Inject the discovered tools into the LLM context
            Temperature = 0.1f
        };

        // The LLM analyzes the prompt, sees the tools, and decides to call the 'get_order_status' tool.
        // It returns a request to call the tool, not the final answer yet.
        var response = await chatClient.GetResponseAsync(userPrompt, chatOptions);

        // 9. Handle Tool Calls (The "Agentic" Part)
        // In a complex flow, the LLM might return a request to call a tool.
        // Microsoft.Extensions.AI handles the automatic execution if we configure it, 
        // but for explicit control in microservices, we often inspect the response.
        // For this specific implementation, we check if the response contains a tool call request.
        
        // Note: The DemoEchoChatClient simulates the LLM requesting the 'get_order_status' tool.
        // If this was a real LLM (like GPT-4), it would internally decide to call the tool 
        // and return the result, or return a ToolCallRequest object.
        
        // Let's simulate the LLM actually executing the tool and getting the result:
        if (response.Text.Contains("get_order_status")) 
        {
            // In a real flow, the library handles this. Here we demonstrate the logic manually 
            // to show what happens under the hood.
            Console.WriteLine("\n[Agent]: I need to check the order database...");
            
            // Simulate calling the tool via the MCP client
            var toolResult = await mcpClient.CallToolAsync("get_order_status", new { OrderId = "67890" });
            
            // Feed the tool result back to the LLM to generate the natural language response
            var finalPrompt = $"User asked: '{userPrompt}'. Tool result: {JsonSerializer.Serialize(toolResult.Content)}. Formulate a helpful response.";
            var finalResponse = await chatClient.GetResponseAsync(finalPrompt, chatOptions);
            
            Console.WriteLine($"\n[Agent]: {finalResponse.Text}");
        }
        else 
        {
            Console.WriteLine($"\n[Agent]: {response.Text}");
        }

        await mcpClient.DisposeAsync();
    }
}

// --- MOCK INFRASTRUCTURE (To make this example runnable without external dependencies) ---

/// <summary>
/// A Mock MCP Server. In production, this would be a separate microservice (e.g., Python or Node.js)
/// running on Kubernetes, exposing tools via SSE or Stdio transport.
/// </summary>
public class MockMcpServer
{
    private readonly Channel<string> _outputChannel;
    public MockMcpServer(Channel<string> outputChannel) => _outputChannel = outputChannel;

    public async Task StartAsync()
    {
        // Simulate the MCP Protocol handshake and Tool Definition
        var initMessage = @"{""jsonrpc"":""2.0"",""id"":1,""result"":{""protocolVersion"":""2024-11-05"",""serverInfo"":{""name"":""OrderService"",""version"":""1.0""},""capabilities"":{}}}";
        await _outputChannel.Writer.WriteAsync(initMessage);

        var toolsList = @"{""jsonrpc"":""2.0"",""id"":2,""result"":{""tools"":[{""name"":""get_order_status"",""description"":""Retrieves the status of a specific order by ID."",""inputSchema"":{""type"":""object"",""properties"":{""OrderId"":{""type"":""string""}}}}]}}";
        await _outputChannel.Writer.WriteAsync(toolsList);

        // Listen for tool calls (Simulated via a simple read loop in a real app)
        // This part is simplified for the single-file example context.
    }
}

/// <summary>
/// A mock IChatClient that simulates an LLM's behavior:
/// 1. Recognizes the intent to check an order.
/// 2. Requests to call the 'get_order_status' tool.
/// </summary>
public class DemoEchoChatClient : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(string prompt, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        // Logic: If the prompt asks about an order, simulate the LLM deciding to use the tool.
        // In a real LLM, this decision is made by the model weights.
        if (prompt.Contains("order") && !prompt.Contains("Tool result:"))
        {
            // The LLM technically returns a request to call the tool.
            // The Microsoft.Extensions.AI library usually handles the execution, 
            // but we are demonstrating the flow here.
            return Task.FromResult(new ChatResponse(
                new ChatMessage(ChatRole.Assistant, "I need to use the get_order_status tool to help you.")
            ));
        }
        
        // If we feed the tool result back, the LLM summarizes it.
        if (prompt.Contains("Tool result:"))
        {
            return Task.FromResult(new ChatResponse(
                new ChatMessage(ChatRole.Assistant, "Your order #67890 is currently 'Shipped'. It will arrive by tomorrow.")
            ));
        }

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "I don't understand.")));
    }

    public async Task<ChatResponse<T>> GetResponseAsync<T>(string prompt, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(prompt, options, cancellationToken);
        return new ChatResponse<T>(response.Messages, response.FinishReason);
    }

    public IAsyncEnumerable<StreamingChatResponseUpdate> GetStreamingResponseAsync(string prompt, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}
