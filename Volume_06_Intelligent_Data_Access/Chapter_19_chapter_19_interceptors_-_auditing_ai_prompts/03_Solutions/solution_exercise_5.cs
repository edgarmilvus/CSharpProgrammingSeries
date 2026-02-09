
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Linq;

// 1. Custom Attribute for Sensitivity
public enum MaskingStrategy { Mask, Hash, Redact }

[AttributeUsage(AttributeTargets.Property)]
public class SensitiveDataAttribute : Attribute
{
    public MaskingStrategy RedactionType { get; set; }
}

// 2. Entity with Sensitivity Metadata
public class PromptLog
{
    public int Id { get; set; }

    [SensitiveData(RedactionType = MaskingStrategy.Redact)]
    public string FinancialData { get; set; } // High sensitivity

    [SensitiveData(RedactionType = MaskingStrategy.Mask)]
    public string UserEmail { get; set; } // Medium sensitivity

    public string GenericNote { get; set; } // Low sensitivity
}

// 3. The Advanced Interceptor
public class SensitivityAuditInterceptor : DbCommandInterceptor
{
    // We need access to the ChangeTracker to inspect the entities being modified.
    // However, DbCommandInterceptor does not directly expose the DbContext or ChangeTracker.
    // To solve this, we usually pass the DbContext to the interceptor, 
    // OR we rely on the CommandText/Parameters to infer data.
    
    // *Constraint Solution*: Since the exercise asks to inspect EntityEntry, 
    // we need to bridge the gap. A common pattern is to register the interceptor 
    // with a reference to the context, or use a service locator (not ideal).
    
    // For this solution, we will simulate the inspection based on the Command Parameters 
    // and a helper method that mimics reflection on the entity type.
    
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        // Check if this is a sensitive operation
        // In a real scenario, we'd map the command back to the entity type.
        // Here we assume if it contains "PromptLogs", we check our mock entity.
        
        if (command.CommandText.Contains("INSERT INTO \"PromptLogs\"") || 
            command.CommandText.Contains("UPDATE \"PromptLogs\""))
        {
            var auditData = InspectAndRedact(command);
            
            // 4. Granular Control: Log to specific sinks
            if (auditData.IsHighSensitivity)
            {
                // Log full (encrypted) to SecureAuditTable (simulated)
                Console.WriteLine($"[SECURE SINK]: {auditData.FullData}");
            }
            else
            {
                // Log generic to standard audit
                Console.WriteLine($"[STANDARD SINK]: {auditData.RedactedData}");
            }
        }

        return base.ReaderExecuting(command, eventData, result);
    }

    private (bool IsHighSensitivity, string FullData, string RedactedData) InspectAndRedact(DbCommand command)
    {
        // Reflection to get properties of PromptLog
        var properties = typeof(PromptLog).GetProperties();
        
        // We iterate parameters (assuming order matches property order for EF Core standard mapping)
        // This is a simplification. In production, we'd map parameter names to properties.
        
        var parameters = command.Parameters.Cast<DbParameter>().ToList();
        
        bool isHigh = false;
        var redactedParams = new System.Text.StringBuilder();

        // Check annotation on the Entity Type (Simulated "Sensitivity" annotation)
        var entityAnnotations = typeof(PromptLog).GetCustomAttributesData();
        // Check for "Sensitivity" = "High" annotation logic here...
        
        foreach (var prop in properties)
        {
            // Skip ID
            if (prop.Name == "Id") continue;

            // Find matching parameter (EF Core uses @p0, @p1...)
            var paramIndex = parameters.FindIndex(p => p.ParameterName.Contains(prop.Name.ToLower()));
            if (paramIndex == -1) continue;

            var value = parameters[paramIndex].Value?.ToString();
            if (string.IsNullOrEmpty(value)) continue;

            // Check for SensitiveDataAttribute
            var attr = prop.GetCustomAttribute<SensitiveDataAttribute>();
            
            if (attr != null)
            {
                if (attr.RedactionType == MaskingStrategy.Redact)
                {
                    isHigh = true; // Triggers secure table
                    value = "[REDACTED]";
                }
                else if (attr.RedactionType == MaskingStrategy.Mask)
                {
                    // Email Masking: user@domain.com -> u***@domain.com
                    value = Regex.Replace(value, @"(?<=.)[^@](?=[^@]*?[^@]$)", "*");
                }
            }

            redactedParams.Append($"{prop.Name}:{value}; ");
        }

        return (isHigh, command.Parameters[0].Value?.ToString(), redactedParams.ToString());
    }
}
