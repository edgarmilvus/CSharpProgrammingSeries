
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

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

// --- Entity and DbContext Definitions ---

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string TenantId { get; set; } = string.Empty;
}

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    
    // We store the tenant ID here to be accessed by the interceptor
    private readonly string? _currentTenantId;

    public AppDbContext(DbContextOptions<AppDbContext> options, string? currentTenantId = null) 
        : base(options)
    {
        _currentTenantId = currentTenantId;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Enable sensitive data logging for debugging (remove in production)
        optionsBuilder.EnableSensitiveDataLogging();
        
        // Register the custom interceptor if a tenant is set
        if (!string.IsNullOrEmpty(_currentTenantId))
        {
            optionsBuilder.AddInterceptors(new TenantContextInterceptor(_currentTenantId));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Optional: Configure the TenantId as a shadow property if not mapped directly
        // But here we map it directly in the Product class.
    }
}

// --- Interceptor to set Session Context ---

public class TenantContextInterceptor : DbConnectionInterceptor
{
    private readonly string _tenantId;

    public TenantContextInterceptor(string tenantId)
    {
        _tenantId = tenantId;
    }

    public override InterceptionResult<DbConnection> ConnectionOpening(
        DbConnection connection, 
        ConnectionEventData eventData, 
        InterceptionResult<DbConnection> result)
    {
        // For synchronous opening
        if (connection.State == System.Data.ConnectionState.Closed)
        {
            connection.Open();
            SetContext(connection);
        }
        return base.ConnectionOpening(connection, eventData, result);
    }

    public override async ValueTask<DbConnection> ConnectionOpeningAsync(
        DbConnection connection, 
        ConnectionEventData eventData, 
        InterceptionResult<DbConnection> result,
        CancellationToken cancellationToken = default)
    {
        // For asynchronous opening
        if (connection.State == System.Data.ConnectionState.Closed)
        {
            await connection.OpenAsync(cancellationToken);
            SetContext(connection);
        }
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    private void SetContext(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        // SQL Server specific command to set session context
        command.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@tenantValue";
        var param = command.CreateParameter();
        param.ParameterName = "@tenantValue";
        param.Value = _tenantId;
        command.Parameters.Add(param);
        command.ExecuteNonQuery();
    }
}

// --- Database Setup Helper (SQL Execution) ---

public static class DatabaseSetup
{
    public static void Initialize(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // 1. Create Table
        var createTableSql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
            CREATE TABLE Products (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100),
                Price DECIMAL(18,2),
                TenantId NVARCHAR(50)
            );";
        ExecuteNonQuery(connection, createTableSql);

        // 2. Create Security Predicate Function
        var createFunctionSql = @"
            CREATE OR ALTER FUNCTION Security.TenantAccessPredicate(@TenantId NVARCHAR(50))
            RETURNS TABLE
            WITH SCHEMABINDING
            AS
            RETURN SELECT 1 AS accessResult
            WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS NVARCHAR(50));";
        ExecuteNonQuery(connection, createFunctionSql);

        // 3. Create Security Policy
        var createPolicySql = @"
            IF NOT EXISTS (SELECT * FROM sys.security_policies WHERE name = 'TenantProductPolicy')
            BEGIN
                CREATE SECURITY POLICY Security.TenantProductPolicy
                ADD FILTER PREDICATE Security.TenantAccessPredicate(TenantId) ON dbo.Products,
                ADD BLOCK PREDICATE Security.TenantAccessPredicate(TenantId) ON dbo.Products
                WITH (STATE = ON);
            END";
        ExecuteNonQuery(connection, createPolicySql);
    }

    private static void ExecuteNonQuery(SqlConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}

// --- Main Program for Testing ---

class Program
{
    static void Main(string[] args)
    {
        // Replace with your actual connection string
        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=MultiTenantRlsDb;Trusted_Connection=True;";

        // Initialize Database Schema and RLS
        DatabaseSetup.Initialize(connectionString);

        // Seed Data (Run once with a context that has access)
        SeedData(connectionString);

        Console.WriteLine("--- Testing Tenant A ---");
        TestTenantAccess(connectionString, "TenantA");

        Console.WriteLine("\n--- Testing Tenant B ---");
        TestTenantAccess(connectionString, "TenantB");

        Console.WriteLine("\n--- Testing Security Breach (No Tenant Context) ---");
        TestBreachAttempt(connectionString);
    }

    static void SeedData(string connectionString)
    {
        // We seed data by temporarily bypassing the interceptor or setting specific context
        // For simplicity, we insert directly via SQL or use a context with specific logic.
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        
        // Clear existing
        ExecuteNonQuery(connection, "DELETE FROM Products");

        // Insert Data
        var products = new[]
        {
            "(1, 'Laptop - TenantA', 1200.00, 'TenantA')",
            "(2, 'Mouse - TenantA', 25.00, 'TenantA')",
            "(3, 'Monitor - TenantB', 300.00, 'TenantB')",
            "(4, 'Keyboard - TenantB', 45.00, 'TenantB')"
        };

        foreach (var p in products)
        {
            ExecuteNonQuery(connection, $"INSERT INTO Products (Id, Name, Price, TenantId) VALUES {p}");
        }
    }

    static void TestTenantAccess(string connectionString, string tenantId)
    {
        // We pass the tenantId to the DbContext, which triggers the Interceptor
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new AppDbContext(options, tenantId);
        
        // EF Core Query
        var products = context.Products.ToList();

        Console.WriteLine($"Found {products.Count} products:");
        foreach (var p in products)
        {
            Console.WriteLine($" - {p.Name} (Price: {p.Price})");
        }
    }

    static void TestBreachAttempt(string connectionString)
    {
        // Create context without tenant ID set
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new AppDbContext(options, null); // No tenant ID
        
        try
        {
            var products = context.Products.ToList();
            Console.WriteLine($"Breach Result: Found {products.Count} products (Expected 0 due to RLS).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred (expected if RLS blocks access strictly): {ex.Message}");
        }
    }

    private static void ExecuteNonQuery(SqlConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
