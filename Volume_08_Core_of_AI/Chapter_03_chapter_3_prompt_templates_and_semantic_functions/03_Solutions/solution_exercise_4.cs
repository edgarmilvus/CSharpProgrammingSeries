
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.SemanticKernel;
using System.ComponentModel;

// Custom Exceptions
public class FraudRiskException : Exception { public FraudRiskException(string msg) : base(msg) { } }
public class InsufficientFundsException : Exception { public InsufficientFundsException(string msg) : base(msg) { } }

// Complex Return Type
public class TransferResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BankingPlugin
{
    // Mock balances
    private readonly Dictionary<string, decimal> _balances = new()
    {
        ["123"] = 1000, // Checking
        ["456"] = 0     // Savings
    };

    [KernelFunction, Description("Transfer funds between accounts")]
    public TransferResult Transfer(
        [Description("Amount to transfer")] decimal amount,
        [Description("Source account number")] string fromAccount,
        [Description("Destination account number")] string toAccount)
    {
        try
        {
            // Validation 1: Fraud Risk
            if (amount > 5000)
                throw new FraudRiskException("Amount exceeds daily limit.");

            // Validation 2: Insufficient Funds
            if (!_balances.ContainsKey(fromAccount) || _balances[fromAccount] < amount)
                throw new InsufficientFundsException($"Insufficient funds in account {fromAccount}.");

            // Execute Transfer
            _balances[fromAccount] -= amount;
            if (_balances.ContainsKey(toAccount))
                _balances[toAccount] += amount;
            else
                _balances[toAccount] = amount;

            return new TransferResult 
            { 
                Success = true, 
                TransactionId = Guid.NewGuid().ToString() 
            };
        }
        catch (FraudRiskException ex)
        {
            return new TransferResult { Success = false, ErrorMessage = ex.Message };
        }
        catch (InsufficientFundsException ex)
        {
            return new TransferResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        
        // Configure AI Connector
        // builder.AddOpenAIChatCompletion(...);
        
        var kernel = builder.Build();

        // Import Plugin
        var plugin = kernel.ImportPluginFromObject(new BankingPlugin(), "Banking");

        // System Prompt
        var systemPrompt = "You are a helpful banking assistant. You have access to a function to transfer funds. " +
                           "You must validate inputs before proceeding. Use the Transfer function for any transfer requests.";

        // 1. Test Success Case
        Console.WriteLine("--- Test Case 1: Valid Transfer ($200) ---");
        var chat = kernel.CreateFunctionFromPrompt(systemPrompt + " User: I want to transfer $200 from checking account 123 to savings account 456.");
        
        // Auto-invocation Configuration
        // In modern SK, auto-invocation is the default behavior when functions are available and the AI requests it.
        // We explicitly set it via ExecutionSettings if needed, but usually the Kernel handles this automatically.
        var settings = new OpenAIPromptExecutionSettings 
        { 
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions 
        };

        var result = await kernel.InvokeAsync(chat, new KernelArguments(settings));
        Console.WriteLine($"Assistant: {result}");

        // 2. Test Fraud Case
        Console.WriteLine("\n--- Test Case 2: Fraud Risk ($6000) ---");
        var chatFraud = kernel.CreateFunctionFromPrompt(systemPrompt + " User: I want to transfer $6000 from checking account 123 to savings account 456.");
        
        var resultFraud = await kernel.InvokeAsync(chatFraud, new KernelArguments(settings));
        Console.WriteLine($"Assistant: {resultFraud}");
    }
}
