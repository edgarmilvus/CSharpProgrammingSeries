
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
using System.IO;
using System.Text;

namespace SyntheticDataSeedingApp
{
    // ============================================================================
    // REAL-WORLD PROBLEM CONTEXT
    // ============================================================================
    // Scenario: A healthcare analytics startup is building a system to predict 
    // patient readmission risks. The ML models require extensive training data, 
    // but using real patient data is strictly prohibited due to HIPAA regulations.
    // 
    // Solution: We will generate a synthetic dataset representing Patients, 
    // Doctors, and Appointments. We must maintain strict referential integrity 
    // (e.g., an appointment must belong to a valid doctor and patient) and 
    // ensure data variety to stress-test the downstream Vector Database 
    // integration for semantic search on doctor notes.
    // 
    // Scope: This console application generates 100 synthetic patients, 
    // 10 doctors, and 500 appointments, saving them to a structured CSV 
    // format that mimics a database export.
    // ============================================================================

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Synthetic Data Generation for Healthcare Analytics...");

            // Configuration: Define how much data to generate
            int patientCount = 100;
            int doctorCount = 10;
            int appointmentCount = 500;

            // 1. Initialize the Seeding Service
            DataSeeder seeder = new DataSeeder();

            // 2. Generate Domain Entities
            // We generate Doctors first as they are a dependency for Appointments.
            List<Doctor> doctors = seeder.GenerateDoctors(doctorCount);
            List<Patient> patients = seeder.GeneratePatients(patientCount);
            List<Appointment> appointments = seeder.GenerateAppointments(appointmentCount, doctors, patients);

            // 3. Export Data (Simulating Database Bulk Insert)
            // In a real scenario, this would be EF Core's BulkInsertAsync.
            // Here, we write to CSV to demonstrate the data structure and integrity.
            string exportPath = Path.Combine(Environment.CurrentDirectory, "synthetic_healthcare_data.csv");
            seeder.ExportToCsv(exportPath, patients, doctors, appointments);

            Console.WriteLine($"Data generation complete. File saved to: {exportPath}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    // ============================================================================
    // DOMAIN MODELS
    // These classes represent the EF Core entities we would typically map to a DB.
    // ============================================================================

    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialty { get; set; }
    }

    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Diagnosis { get; set; } // Used for Vector DB embedding later
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }      // Foreign Key
        public int PatientId { get; set; }     // Foreign Key
        public DateTime Date { get; set; }
        public string Notes { get; set; }      // Unstructured text for RAG/Vector testing
    }

    // ============================================================================
    // DATA SEEDER LOGIC
    // Handles the generation of synthetic data using deterministic randomness.
    // ============================================================================

    public class DataSeeder
    {
        // Pseudo-random number generator for deterministic data generation
        private Random _random = new Random(12345);

        // Helper arrays for generating names and specialties
        private string[] _firstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda" };
        private string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
        private string[] _specialties = { "Cardiology", "Neurology", "Pediatrics", "Orthopedics", "General Practice" };
        private string[] _diagnoses = { "Hypertension", "Type 2 Diabetes", "Asthma", "Osteoarthritis", "Migraine" };
        
        // Synthetic text templates for RAG (Retrieval-Augmented Generation) testing
        private string[] _noteTemplates = {
            "Patient presented with mild symptoms. Recommended follow-up in 2 weeks.",
            "Routine checkup. Vitals are stable. Prescribed medication.",
            "Patient reported severe pain. Immediate intervention required.",
            "Post-operative check. Wound healing well.",
            "Annual physical. Blood work ordered."
        };

        // ------------------------------------------------------------------------
        // METHOD: GenerateDoctors
        // Objective: Create a list of distinct medical professionals.
        // Logic: Loop through count, generate random names from pools, assign specialty.
        // ------------------------------------------------------------------------
        public List<Doctor> GenerateDoctors(int count)
        {
            List<Doctor> doctors = new List<Doctor>();

            for (int i = 0; i < count; i++)
            {
                Doctor doc = new Doctor
                {
                    Id = i + 1, // Simple sequential ID
                    Name = $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}",
                    Specialty = _specialties[_random.Next(_specialties.Length)]
                };
                doctors.Add(doc);
            }
            return doctors;
        }

        // ------------------------------------------------------------------------
        // METHOD: GeneratePatients
        // Objective: Create a list of synthetic patients with varied demographics.
        // Logic: Similar to doctors, but includes age generation and diagnosis.
        // ------------------------------------------------------------------------
        public List<Patient> GeneratePatients(int count)
        {
            List<Patient> patients = new List<Patient>();

            for (int i = 0; i < count; i++)
            {
                Patient pat = new Patient
                {
                    Id = i + 1,
                    Name = $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}",
                    Age = _random.Next(18, 90), // Random age between 18 and 90
                    Diagnosis = _diagnoses[_random.Next(_diagnoses.Length)]
                };
                patients.Add(pat);
            }
            return patients;
        }

        // ------------------------------------------------------------------------
        // METHOD: GenerateAppointments
        // Objective: Create relational data linking Doctors and Patients.
        // CRITICAL: Ensures referential integrity by selecting IDs from existing lists.
        // ------------------------------------------------------------------------
        public List<Appointment> GenerateAppointments(int count, List<Doctor> doctors, List<Patient> patients)
        {
            List<Appointment> appointments = new List<Appointment>();

            for (int i = 0; i < count; i++)
            {
                // Select random doctor and patient from the provided lists
                // This guarantees the Foreign Keys exist in our generated dataset
                Doctor randomDoc = doctors[_random.Next(doctors.Count)];
                Patient randomPat = patients[_random.Next(patients.Count)];

                // Generate a random date within the last year
                DateTime randomDate = DateTime.Now.AddDays(-_random.Next(365));

                // Construct a unique note combining the template with specific data
                string baseNote = _noteTemplates[_random.Next(_noteTemplates.Length)];
                string syntheticNote = $"{baseNote} Patient: {randomPat.Diagnosis}. Doctor: {randomDoc.Specialty}.";

                Appointment appt = new Appointment
                {
                    Id = i + 1,
                    DoctorId = randomDoc.Id,
                    PatientId = randomPat.Id,
                    Date = randomDate,
                    Notes = syntheticNote
                };
                appointments.Add(appt);
            }
            return appointments;
        }

        // ------------------------------------------------------------------------
        // METHOD: ExportToCsv
        // Objective: Persist data to a file format suitable for bulk import tools.
        // Logic: Use StreamWriter to write headers and row data.
        // ------------------------------------------------------------------------
        public void ExportToCsv(string path, List<Patient> patients, List<Doctor> doctors, List<Appointment> appointments)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                // Write Header
                writer.WriteLine("EntityType,Id,Name,Specialty,Age,Diagnosis,DoctorId,PatientId,Date,Notes");

                // Write Doctors
                foreach (var doc in doctors)
                {
                    writer.WriteLine($"Doctor,{doc.Id},{doc.Name},{doc.Specialty},,,,,");
                }

                // Write Patients
                foreach (var pat in patients)
                {
                    writer.WriteLine($"Patient,{pat.Id},{pat.Name},,{pat.Age},{pat.Diagnosis},,,,");
                }

                // Write Appointments (Relational Data)
                foreach (var appt in appointments)
                {
                    // Formatting date to ISO 8601 standard for consistency
                    writer.WriteLine($"Appointment,{appt.Id},,,,,{appt.DoctorId},{appt.PatientId},{appt.Date:yyyy-MM-dd},\"{appt.Notes}\"");
                }
            }
        }
    }
}
