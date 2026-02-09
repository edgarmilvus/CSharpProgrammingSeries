
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

// ---------------------------------------------------------
// REAL-WORLD CONTEXT
// ---------------------------------------------------------
// Problem: A small logistics company manages a fleet of delivery vehicles.
// They need a console application to track vehicle maintenance logs.
// The core challenge is handling concurrent updates:
// 1. A mechanic updates the "LastServiceDate" of a vehicle.
// 2. A driver updates the "Mileage" of the same vehicle.
// 3. We must ensure data integrity using EF Core's Change Tracking
//    and Optimistic Concurrency strategies taught in Chapter 5.

namespace LogisticsManagement
{
    // ---------------------------------------------------------
    // DOMAIN MODEL (Chapter 4: Modeling Data)
    // ---------------------------------------------------------
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public double CurrentMileage { get; set; }
        public DateTime? LastServiceDate { get; set; }

        // Concurrency Token: Used for Optimistic Concurrency checks
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public List<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
    }

    public class MaintenanceLog
    {
        [Key]
        public int LogId { get; set; }
        public string Description { get; set; }
        public DateTime DatePerformed { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }

    // ---------------------------------------------------------
    // DATA CONTEXT (Chapter 5: DbContext & Change Tracking)
    // ---------------------------------------------------------
    public class LogisticsContext : DbContext
    {
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<MaintenanceLog> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using InMemory for demonstration purposes to focus on logic
            optionsBuilder.UseInMemoryDatabase("LogisticsDb");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring Optimistic Concurrency on the Vehicle entity
            modelBuilder.Entity<Vehicle>()
                .Property(p => p.RowVersion)
                .IsConcurrencyToken();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Logistics Maintenance System ===");
            Console.WriteLine("Demonstrating Change Tracking & Concurrency\n");

            // 1. SETUP: Initialize Database and Seed Data
            int vehicleId = SeedDatabase();

            // 2. SCENARIO: Concurrent Modifications
            SimulateConcurrentUpdates(vehicleId);

            Console.WriteLine("\nApplication finished. Press any key to exit.");
            Console.ReadKey();
        }

        static int SeedDatabase()
        {
            using (var context = new LogisticsContext())
            {
                // Ensure database is created
                context.Database.EnsureCreated();

                // Check if data already exists to avoid duplicates on restart
                if (!context.Vehicles.Any())
                {
                    var vehicle = new Vehicle
                    {
                        LicensePlate = "XYZ-1234",
                        CurrentMileage = 50000,
                        LastServiceDate = new DateTime(2023, 1, 15)
                    };

                    context.Vehicles.Add(vehicle);
                    context.SaveChanges(); // Persist initial state

                    Console.WriteLine($"Seeded Vehicle: {vehicle.LicensePlate} (ID: {vehicle.VehicleId})");
                    return vehicle.VehicleId;
                }
                else
                {
                    return context.Vehicles.First().VehicleId;
                }
            }
        }

        static void SimulateConcurrentUpdates(int vehicleId)
        {
            // -----------------------------------------------------
            // BLOCK 1: Mechanic updates the vehicle (First Context)
            // -----------------------------------------------------
            Console.WriteLine("\n--- Operation 1: Mechanic Service Update ---");
            using (var mechanicContext = new LogisticsContext())
            {
                // Fetching the entity. EF Core starts Change Tracking here.
                // EntityState becomes 'Unchanged'.
                var vehicle = mechanicContext.Vehicles.Find(vehicleId);

                Console.WriteLine($"Initial State: Mileage={vehicle.CurrentMileage}, LastService={vehicle.LastServiceDate:yyyy-MM-dd}");

                // Modify properties. EF Core detects changes.
                // EntityState becomes 'Modified'.
                vehicle.LastServiceDate = DateTime.Now;
                vehicle.CurrentMileage += 100; // Mechanic drove it after service

                // Simulate a delay (e.g., network latency or heavy processing)
                System.Threading.Thread.Sleep(1000);

                // Save changes to the database.
                // EF Core generates an UPDATE statement including the RowVersion check.
                mechanicContext.SaveChanges();
                Console.WriteLine("Mechanic update saved successfully.");
            }

            // -----------------------------------------------------
            // BLOCK 2: Driver updates the vehicle (Second Context)
            // -----------------------------------------------------
            Console.WriteLine("\n--- Operation 2: Driver Mileage Update ---");
            using (var driverContext = new LogisticsContext())
            {
                // Fetching the entity. This retrieves the UPDATED RowVersion from the DB.
                var vehicle = driverContext.Vehicles.Find(vehicleId);

                Console.WriteLine($"Current DB State: Mileage={vehicle.CurrentMileage}, LastService={vehicle.LastServiceDate:yyyy-MM-dd}");

                // Driver updates mileage (ignoring the service date).
                vehicle.CurrentMileage += 50;

                // Attempt to save.
                try
                {
                    // This will trigger the concurrency check.
                    // The UPDATE statement WHERE clause includes: RowVersion = @original_row_version
                    // Since MechanicContext changed the row version, this will fail.
                    driverContext.SaveChanges();
                    Console.WriteLine("Driver update saved successfully.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine("CONCURRENCY CONFLICT DETECTED!");
                    Console.WriteLine("Error: The vehicle data was modified by another user (Mechanic) before this save.");

                    // -----------------------------------------------------
                    // BLOCK 3: Resolving the Conflict (Chapter 5 Strategy)
                    // -----------------------------------------------------
                    HandleConcurrencyConflict(ex, vehicleId);
                }
            }
        }

        static void HandleConcurrencyConflict(DbUpdateConcurrencyException exception, int vehicleId)
        {
            Console.WriteLine("\n--- Executing Conflict Resolution Strategy ---");

            using (var resolutionContext = new LogisticsContext())
            {
                // Retrieve the current database values
                var databaseVehicle = resolutionContext.Vehicles.Find(vehicleId);

                // Retrieve the values the driver attempted to save (stored in the exception)
                // Note: In a real app, we might use a detached entity, but for this console demo
                // we will reconstruct the intended state based on the exception's entries.
                
                var driverEntry = exception.Entries.Single();
                var proposedValues = driverEntry.CurrentValues;
                
                Console.WriteLine($"Database Values: Mileage={databaseVehicle.CurrentMileage}, Service={databaseVehicle.LastServiceDate:yyyy-MM-dd}");
                Console.WriteLine($"Driver Proposed: Mileage={proposedValues["CurrentMileage"]}, Service={proposedValues["LastServiceDate"]}");

                // STRATEGY: Client Wins (Overwrite Database)
                // We take the driver's mileage but keep the mechanic's service date.
                // To do this safely, we reload the entity into the tracked context.
                var vehicleToUpdate = resolutionContext.Vehicles.Find(vehicleId);

                // Update the mileage to what the driver intended
                vehicleToUpdate.CurrentMileage = (double)proposedValues["CurrentMileage"];
                
                // We explicitly DO NOT update LastServiceDate, preserving the mechanic's change.
                
                Console.WriteLine("Resolution: Merging Driver's Mileage into current DB state.");
                
                // Save changes. This generates a new UPDATE with the NEW RowVersion.
                resolutionContext.SaveChanges();
                Console.WriteLine("Conflict resolved and data saved.");
            }
        }
    }
}
