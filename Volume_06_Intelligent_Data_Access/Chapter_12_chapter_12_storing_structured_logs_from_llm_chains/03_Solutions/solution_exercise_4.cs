
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Text.Json;
using System.Linq;
using System.Reflection;

// 1. Encryption Abstraction
public interface IEncryptionProvider
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

// Mock Implementation
public class AesEncryptionProvider : IEncryptionProvider
{
    public string Encrypt(string plainText) => $"ENCRYPTED_{plainText}"; // Simplified
    public string Decrypt(string cipherText) => cipherText.Replace("ENCRYPTED_", "");
}

// Attribute for marking sensitive fields
[AttributeUsage(AttributeTargets.Property)]
public class SensitiveDataAttribute : Attribute { }

// 2. Value Converter
public class JsonValueConverter<T> : ValueConverter<T, string>
{
    public JsonValueConverter(IEncryptionProvider? encryptionProvider = null)
        : base(
            // Convert to String (Database)
            v => Serialize(v, encryptionProvider),
            // Convert from String (Database)
            v => Deserialize<T>(v, encryptionProvider),
            // Ensure converter is unique per provider if needed
            mappingHints: default
        ) { }

    private static string Serialize(T value, IEncryptionProvider? encryptionProvider)
    {
        if (value == null) return "{}";

        // Reflection to check for [SensitiveData] on properties
        var json = JsonSerializer.Serialize(value);
        
        if (encryptionProvider != null)
        {
            // In a real scenario, we would parse the JSON, encrypt specific fields, 
            // and re-serialize. For this exercise, we simulate encrypting the whole payload
            // if it contains sensitive markers, or we assume the DTO has logic.
            // Here is a simplified approach:
            var type = typeof(T);
            var hasSensitive = type.GetProperties()
                .Any(p => p.GetCustomAttribute<SensitiveDataAttribute>() != null);
            
            if (hasSensitive)
            {
                return encryptionProvider.Encrypt(json);
            }
        }
        return json;
    }

    private static T Deserialize(string json, IEncryptionProvider? encryptionProvider)
    {
        if (string.IsNullOrEmpty(json)) return default!;

        if (encryptionProvider != null && json.StartsWith("ENCRYPTED_"))
        {
            json = encryptionProvider.Decrypt(json);
        }
        
        return JsonSerializer.Deserialize<T>(json)!;
    }
}

// 3. DbContext Configuration
public class PortableLogContext : DbContext
{
    public DbSet<ChainExecution> Executions { get; set; }
    public DbSet<Step> Steps { get; set; }
    
    private readonly IEncryptionProvider _encryptionProvider;

    public PortableLogContext(DbContextOptions<PortableLogContext> options, IEncryptionProvider encryptionProvider)
        : base(options)
    {
        _encryptionProvider = encryptionProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Step entity
        modelBuilder.Entity<Step>(entity =>
        {
            // Apply the Value Converter with the encryption provider
            var converter = new JsonValueConverter<object>(_encryptionProvider);

            entity.Property(s => s.InputPayload)
                  .HasConversion(converter);
            
            entity.Property(s => s.OutputPayload)
                  .HasConversion(converter);

            // Provider-Specific Type Configuration
            if (Database.IsSqlServer())
            {
                entity.Property(s => s.InputPayload).HasColumnType("nvarchar(max)");
                entity.Property(s => s.OutputPayload).HasColumnType("nvarchar(max)");
            }
            else if (Database.IsNpgsql())
            {
                entity.Property(s => s.InputPayload).HasColumnType("jsonb");
                entity.Property(s => s.OutputPayload).HasColumnType("jsonb");
            }
        });
    }

    // 4. Schema Verification
    public void VerifySchemaCompatibility()
    {
        var model = this.Model;
        var entityTypes = model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            Console.WriteLine($"Entity: {entityType.Name}");
            foreach (var prop in entityType.GetProperties())
            {
                var column = prop.GetColumnBaseName(); // Requires EF Core 7+
                var typeName = prop.GetColumnType();
                Console.WriteLine($"  - Property: {prop.Name}, Column: {column}, Type: {typeName}");
            }
        }
    }
}
