
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;

// ==========================================
// APPLICATION CONTEXT: BANKING TRANSACTION RECONCILIATION
// ==========================================
// This application simulates a critical financial operation where a user attempts
// to transfer funds between accounts. The system must also update a separate
// "Vector Search Index" (simulated) to allow for natural language queries like
// "Show me recent large transfers from Account X".
//
// The core challenge, addressed in this code, is ensuring that the money transfer
// in the relational store and the index update in the vector store happen
// atomically. If the vector update fails, the money transfer must be rolled back
// to prevent data inconsistency.

public class Program
{
    // Main Entry Point
    public static void Main(string[] args)
    {
        Console.WriteLine("Initializing Banking System with Vector Memory Integration...");
        
        // 1. Setup Infrastructure
        RelationalDatabase relationalDb = new RelationalDatabase();
        VectorMemoryStore vectorStore = new VectorMemoryStore();
        
        // 2. Create Test Data
        relationalDb.CreateAccount("ACC-001", 1000.00m); // Sender
        relationalDb.CreateAccount("ACC-002", 500.00m);  // Receiver
        
        // 3. Execute the Transaction Scenario
        // Scenario: Transfer 200.00 from ACC-001 to ACC-002
        // We simulate a failure in the Vector Store to demonstrate the Rollback.
        ExecuteBankingTransaction(relationalDb, vectorStore, "ACC-001", "ACC-002", 200.00m);
        
        // 4. Verify Results
        Console.WriteLine("\n--- Final State Verification ---");
        Console.WriteLine("Account ACC-001 Balance: " + relationalDb.GetBalance("ACC-001"));
        Console.WriteLine("Account ACC-002 Balance: " + relationalDb.GetBalance("ACC-002"));
        Console.WriteLine("Vector Store Log Count: " + vectorStore.GetLogCount());
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    // ==========================================
    // THE UNIT OF WORK PATTERN IMPLEMENTATION
    // ==========================================
    // This method acts as the "Unit of Work". It coordinates the changes across
    // two different systems (Relational DB and Vector Store).
    public static void ExecuteBankingTransaction(RelationalDatabase db, VectorMemoryStore vectorStore, string fromAcc, string toAcc, decimal amount)
    {
        Console.WriteLine($"\n[Operation] Initiating Transfer: {fromAcc} -> {toAcc} : ${amount}");
        
        // We instantiate a specific Unit of Work for this operation.
        BankingTransactionUnit workUnit = new BankingTransactionUnit(db, vectorStore, fromAcc, toAcc, amount);
        
        try
        {
            // Step 1: Register changes in the Unit of Work (Staging)
            // No physical changes to the database happen here yet.
            workUnit.StageRelationalChanges();
            workUnit.StageVectorChanges();
            
            // Step 2: Commit the Unit of Work
            // This is where the transaction actually executes.
            workUnit.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Transaction failed: {ex.Message}");
            Console.WriteLine("[Action] Rolling back changes...");
            
            // Step 3: Compensating Transaction (Rollback)
            // If any part fails, we execute the rollback logic to restore consistency.
            workUnit.Rollback();
        }
    }
}

// ==========================================
// CORE CONCEPT: UNIT OF WORK CLASS
// ==========================================
// This class encapsulates the logic for coordinating the distributed transaction.
public class BankingTransactionUnit
{
    private readonly RelationalDatabase _relationalDb;
    private readonly VectorMemoryStore _vectorStore;
    
    // State tracking for Rollback capability
    private bool _isRelationalCommitted = false;
    private bool _isVectorCommitted = false;
    
    // Transaction Data
    private string _fromAcc;
    private string _toAcc;
    private decimal _amount;

    public BankingTransactionUnit(RelationalDatabase db, VectorMemoryStore vector, string from, string to, decimal amount)
    {
        _relationalDb = db;
        _vectorStore = vector;
        _fromAcc = from;
        _toAcc = to;
        _amount = amount;
    }

    // STAGING: Prepare the Relational Data
    // We calculate the new balances but do not save them yet.
    // In a real system, this might involve checking constraints or locking rows.
    public void StageRelationalChanges()
    {
        Console.WriteLine("   [Stage] Calculating Relational Balances...");
        // Logic check: Does sender have enough money?
        decimal senderBalance = _relationalDb.GetBalance(_fromAcc);
        if (senderBalance < _amount)
        {
            throw new InvalidOperationException("Insufficient funds in sender account.");
        }
    }

    // STAGING: Prepare the Vector Data
    // We prepare the text description for the AI search index.
    public void StageVectorChanges()
    {
        Console.WriteLine("   [Stage] Preparing Vector Embeddings for Search...");
    }

    // COMMIT: The Critical Phase
    // We attempt to persist changes. Order matters.
    public void Commit()
    {
        Console.WriteLine("   [Commit] Phase Started...");
        
        // Strategy: Update the Vector Store FIRST, then the Relational DB.
        // Why? If the Vector Store is down (external dependency), the Relational DB
        // remains untouched. We fail fast before moving money.
        
        try 
        {
            // 1. Update Vector Store (External System)
            string description = $"Transfer of {_amount} from {_fromAcc} to {_toAcc}";
            _vectorStore.AddLog(description);
            _isVectorCommitted = true; // Mark success
            Console.WriteLine("   [Commit] Vector Store updated.");
            
            // 2. Update Relational Database (Primary System)
            _relationalDb.Debit(_fromAcc, _amount);
            _relationalDb.Credit(_toAcc, _amount);
            _isRelationalCommitted = true; // Mark success
            Console.WriteLine("   [Commit] Relational Accounts updated.");
            
            Console.WriteLine("   [Success] Transaction Committed Successfully.");
        }
        catch (Exception ex)
        {
            // If any step fails, we throw to trigger the catch block in Main
            throw new Exception($"Commit failed during execution: {ex.Message}");
        }
    }

    // COMPENSATING TRANSACTION (ROLLBACK)
    // This is the logic to undo changes if the transaction fails midway.
    // It checks which parts were committed and reverses them.
    public void Rollback()
    {
        Console.WriteLine("   [Rollback] Executing Compensating Logic...");
        
        // 1. If Relational DB was updated, we must reverse the money transfer.
        if (_isRelationalCommitted)
        {
            Console.WriteLine("   [Rollback] Reversing Debit/Credit on Relational DB...");
            _relationalDb.Credit(_fromAcc, _amount); // Give money back to sender
            _relationalDb.Debit(_toAcc, _amount);    // Take money back from receiver
        }
        else
        {
            Console.WriteLine("   [Rollback] Relational DB was not touched.");
        }

        // 2. If Vector Store was updated, we attempt to remove the log.
        // Note: In distributed systems, "exact" rollback is hard. We might append a correction log.
        if (_isVectorCommitted)
        {
            Console.WriteLine("   [Rollback] Removing log from Vector Store...");
            _vectorStore.RemoveLastLog();
        }
        else
        {
            Console.WriteLine("   [Rollback] Vector Store was not touched.");
        }
        
        Console.WriteLine("   [Rollback] Complete. System State Restored.");
    }
}

// ==========================================
// MOCK INFRASTRUCTURE (Simulating External Systems)
// ==========================================

// Simulates a SQL Database (Relational Store)
public class RelationalDatabase
{
    private Dictionary<string, decimal> _accounts = new Dictionary<string, decimal>();

    public void CreateAccount(string id, decimal initialBalance)
    {
        _accounts[id] = initialBalance;
    }

    public decimal GetBalance(string id)
    {
        return _accounts.ContainsKey(id) ? _accounts[id] : 0;
    }

    public void Debit(string id, decimal amount)
    {
        // Simulate potential concurrency issue or DB failure
        if (!_accounts.ContainsKey(id)) throw new Exception("Account not found.");
        if (_accounts[id] < amount) throw new Exception("Logic Error: Insufficient funds at Debit execution.");
        
        _accounts[id] -= amount;
    }

    public void Credit(string id, decimal amount)
    {
        if (!_accounts.ContainsKey(id)) throw new Exception("Account not found.");
        _accounts[id] += amount;
    }
}

// Simulates a Vector Database / AI Memory Store
public class VectorMemoryStore
{
    private List<string> _logs = new List<string>();
    private Random _random = new Random();

    public void AddLog(string description)
    {
        // Simulate random failure of the external Vector Store (e.g., network timeout)
        // This demonstrates the need for the Unit of Work pattern.
        if (_random.Next(0, 10) < 3) // 30% chance of failure
        {
            throw new Exception("Vector Store Connection Timeout");
        }
        
        _logs.Add(description);
    }

    public void RemoveLastLog()
    {
        if (_logs.Count > 0)
        {
            _logs.RemoveAt(_logs.Count - 1);
        }
    }

    public int GetLogCount()
    {
        return _logs.Count;
    }
}
