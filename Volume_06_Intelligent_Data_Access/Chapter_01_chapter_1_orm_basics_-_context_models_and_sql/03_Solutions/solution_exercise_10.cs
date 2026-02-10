
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

// Source File: solution_exercise_10.cs
// Description: Solution for Exercise 10
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueConversion;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

public class SecureDbContext : DbContext
{
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Custom Value Converter for Encryption
        var encryptor = new VectorEncryptionConverter();

        modelBuilder.Entity<Document>()
            .Property(d => d.Vector)
            .HasConversion(encryptor)
            .Metadata.SetValueConverter(encryptor);
    }
}

// 2. Custom Value Converter (Transparent Encryption)
public class VectorEncryptionConverter : ValueConverter<float[], byte[]>
{
    private static readonly byte[] Key = new byte[32]; // Load from Key Vault!
    private static readonly byte[] IV = new byte[16];

    public VectorEncryptionConverter() 
        : base(
            // Convert to DB: Encrypt
            v => Encrypt(v),
            // Convert from DB: Decrypt
            v => Decrypt(v)
        )
    {
    }

    private static byte[] Encrypt(float[] vector)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        
        var bytes = vector.SelectMany(BitConverter.GetBytes).ToArray();
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
    }

    private static float[] Decrypt(byte[] cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

        // Convert back to floats
        var floats = new float[plainBytes.Length / 4];
        for (int i = 0; i < floats.Length; i++)
        {
            floats[i] = BitConverter.ToSingle(plainBytes, i * 4);
        }
        return floats;
    }
}

// 3. Homomorphic Considerations (Conceptual)
// Standard AES is NOT homomorphic. To calculate similarity on encrypted data,
// we must either:
// A) Decrypt in memory (what the converter does above) - Vulnerable to memory dumps.
// B) Use Fully Homomorphic Encryption (FHE) - Extremely slow, not practical for vectors yet.
// C) Use a Secure Enclave (Azure SQL Always Encrypted with secure enclaves).

public class HomomorphicDemo
{
    // Placeholder for theoretical FHE implementation
    public double EncryptedSimilarity(byte[] encryptedA, byte[] encryptedB)
    {
        // In a real FHE system, this would perform math on ciphertexts.
        // Currently, we must rely on the ValueConverter to decrypt before calculation.
        // However, we can optimize by decrypting ONLY the vectors needed for the top-K result
        // rather than the whole table.
        throw new NotImplementedException("FHE is not yet viable for high-dimensional vectors.");
    }
}

// 4. Audit Logging (Triggered via Interceptor or SaveChanges)
public class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified && e.Entity is Document);

        foreach (var entry in entries)
        {
            // Log that a vector was potentially accessed or modified
            Console.WriteLine($"AUDIT: Vector modified for Doc {entry.Property("Id").CurrentValue}");
        }

        return result;
    }
}
