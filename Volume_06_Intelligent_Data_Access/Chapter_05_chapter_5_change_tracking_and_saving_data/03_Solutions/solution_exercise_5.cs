
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.EntityFrameworkCore;

public class Document
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string LastModifiedBy { get; set; } = string.Empty;
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

public class ConflictResolutionService
{
    public enum ResolutionStrategy { ClientWins, DatabaseWins, Merge }

    public async Task HandleDocumentUpdateAsync(int documentId, string newContent, string user, ResolutionStrategy strategy)
    {
        using var userContext = new DocumentContext();
        
        // 1. Load the document (User A's view)
        var userDoc = await userContext.Documents.FindAsync(documentId);
        if (userDoc == null) return;

        // Simulate User B changing the data in the DB
        await SimulateExternalChange(documentId);

        // Modify User A's document
        userDoc.Content = newContent;
        userDoc.LastModifiedBy = user;

        try
        {
            await userContext.SaveChangesAsync();
            Console.WriteLine("Saved successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"Conflict detected! Resolving with {strategy} strategy...");
            
            // 4. Inspect Exception.Entries
            var entry = ex.Entries.Single();
            
            // 5. Get Database Values (User B's changes)
            var databaseValues = await entry.GetDatabaseValuesAsync();
            
            // Get Client Values (User A's changes)
            var clientValues = entry.CurrentValues;
            
            object resolvedValues;

            // 6, 7, 8. Implement Strategies
            switch (strategy)
            {
                case ResolutionStrategy.ClientWins:
                    // Keep User A's values. 
                    // We must update the OriginalValues to the DB values to bypass the check
                    entry.OriginalValues.SetValues(databaseValues);
                    await userContext.SaveChangesAsync();
                    break;

                case ResolutionStrategy.DatabaseWins:
                    // Discard User A's changes, accept DB values
                    entry.CurrentValues.SetValues(databaseValues);
                    await userContext.SaveChangesAsync();
                    break;

                case ResolutionStrategy.Merge:
                    // 8. Custom Merge Logic
                    // Example: Concatenate content if LastModifiedBy differs
                    var dbDoc = databaseValues.ToObject() as Document;
                    var clientDoc = clientValues.ToObject() as Document;

                    if (dbDoc != null && clientDoc != null && dbDoc.LastModifiedBy != clientDoc.LastModifiedBy)
                    {
                        // Create a merged object
                        var merged = new Document 
                        { 
                            Id = clientDoc.Id,
                            LastModifiedBy = clientDoc.LastModifiedBy, // Keep client's user
                            // Merge content
                            Content = $"[Merged: User {dbDoc.LastModifiedBy} said: {dbDoc.Content}] \n User {clientDoc.LastModifiedBy} said: {clientDoc.Content}",
                            RowVersion = dbDoc.RowVersion // Keep current DB version
                        };

                        // 9. Re-save using SetValues
                        // This updates the tracked entity with our merged data
                        entry.CurrentValues.SetValues(merged);
                        
                        // We must also update the OriginalValues to the current DB state 
                        // (which we just merged into Current) to allow the save
                        entry.OriginalValues.SetValues(databaseValues);
                        
                        await userContext.SaveChangesAsync();
                    }
                    break;
            }
        }
    }

    private async Task SimulateExternalChange(int id)
    {
        using var context = new DocumentContext();
        var doc = await context.Documents.FindAsync(id);
        if (doc != null)
        {
            doc.Content = "External change by User B";
            doc.LastModifiedBy = "User B";
            await context.SaveChangesAsync();
            Console.WriteLine("External change committed.");
        }
    }
}

public class DocumentContext : DbContext
{
    public DbSet<Document> Documents { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("YourConnectionStringHere");
}
