
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

// =============================================
// ADVANCED APPLICATION SCRIPT: TRAINING DATA PERSISTENCE
// =============================================
// This script demonstrates a complete, real-world application:
// A simple fitness tracker that saves and loads workout sessions
// to/from a text file. We'll use ONLY concepts from Chapters 1-24.
//
// PROBLEM: A personal trainer needs to track client workouts.
// Each session has: Date, Exercise Name, Sets, Reps, Weight (lbs).
// The data must survive program restarts.
//
// SOLUTION: We'll use System.IO.File to write/read a simple
// text file where each line represents one workout session.
// =============================================

using System;
using System.IO; // Required for File operations (Chapter 24)
using System.Collections.Generic; // Required for List<T> (Chapter 20)

// =============================================
// CLASS DEFINITION (Chapter 16)
// Represents a single workout session.
// =============================================
public class WorkoutSession
{
    // Fields (Chapter 17)
    // We use public properties for data access, but fields for internal storage.
    // This is a common pattern in C#.
    private string _date;
    private string _exerciseName;
    private int _sets;
    private int _reps;
    private double _weight;

    // Properties (Chapter 17)
    // These allow controlled access to the fields.
    public string Date
    {
        get { return _date; }
        set { _date = value; }
    }

    public string ExerciseName
    {
        get { return _exerciseName; }
        set { _exerciseName = value; }
    }

    public int Sets
    {
        get { return _sets; }
        set { _sets = value; }
    }

    public int Reps
    {
        get { return _reps; }
        set { _reps = value; }
    }

    public double Weight
    {
        get { return _weight; }
        set { _weight = value; }
    }

    // Constructor (Chapter 18)
    // Initializes a new WorkoutSession object with provided values.
    public WorkoutSession(string date, string exerciseName, int sets, int reps, double weight)
    {
        // 'this' keyword refers to the current instance.
        this.Date = date;
        this.ExerciseName = exerciseName;
        this.Sets = sets;
        this.Reps = reps;
        this.Weight = weight;
    }

    // Method (Chapter 13, 14)
    // Returns a formatted string suitable for saving to a text file.
    // Format: Date|Exercise|Sets|Reps|Weight
    // The pipe character '|' is used as a delimiter.
    public string ToFileFormat()
    {
        // String Interpolation (Chapter 3)
        return $"{this.Date}|{this.ExerciseName}|{this.Sets}|{this.Reps}|{this.Weight}";
    }

    // Method (Chapter 13, 14)
    // Displays the workout details to the console.
    public void Display()
    {
        Console.WriteLine("---------------------------------");
        Console.WriteLine($"Date: {this.Date}");
        Console.WriteLine($"Exercise: {this.ExerciseName}");
        Console.WriteLine($"Sets: {this.Sets}, Reps: {this.Reps}");
        Console.WriteLine($"Weight: {this.Weight} lbs");
        Console.WriteLine("---------------------------------");
    }
}

// =============================================
// MAIN PROGRAM CLASS
// =============================================
public class Program
{
    // Static field (Chapter 19) to hold our list of workouts in memory.
    // We use List<WorkoutSession> (Chapter 20) because we don't know
    // how many workouts the user will enter.
    private static List<WorkoutSession> workoutLog = new List<WorkoutSession>();

    // Static field (Chapter 19) for the file path.
    // This makes it easy to change the filename in one place.
    private static string filePath = "workout_data.txt";

    // =============================================
    // MAIN METHOD (Entry Point)
    // =============================================
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Fitness Tracker Application ===");
        Console.WriteLine("This app saves and loads workout data to a text file.");
        Console.WriteLine();

        // Main application loop (Chapter 8)
        // Keeps running until the user chooses to exit.
        bool running = true;
        while (running)
        {
            Console.WriteLine("\n--- MAIN MENU ---");
            Console.WriteLine("1. Add a new workout");
            Console.WriteLine("2. View all workouts (in memory)");
            Console.WriteLine("3. Save workouts to file");
            Console.WriteLine("4. Load workouts from file");
            Console.WriteLine("5. Clear workouts from memory");
            Console.WriteLine("6. Exit");
            Console.Write("Enter your choice (1-6): ");

            // Read user input (Chapter 5)
            string choice = Console.ReadLine();

            // Use if-else if-else (Chapter 6) to handle menu selection.
            if (choice == "1")
            {
                AddWorkout();
            }
            else if (choice == "2")
            {
                ViewWorkouts();
            }
            else if (choice == "3")
            {
                SaveToFile();
            }
            else if (choice == "4")
            {
                LoadFromFile();
            }
            else if (choice == "5")
            {
                ClearMemory();
            }
            else if (choice == "6")
            {
                Console.WriteLine("Goodbye!");
                running = false; // Exit the while loop
            }
            else
            {
                Console.WriteLine("Invalid choice. Please try again.");
            }
        }
    }

    // =============================================
    // METHOD: AddWorkout
    // Handles user input for creating a new WorkoutSession.
    // =============================================
    public static void AddWorkout()
    {
        Console.WriteLine("\n--- Adding New Workout ---");

        // Get Date
        Console.Write("Enter Date (e.g., 2023-10-27): ");
        string date = Console.ReadLine();

        // Get Exercise Name
        Console.Write("Enter Exercise Name (e.g., Bench Press): ");
        string name = Console.ReadLine();

        // Get Sets (with validation loop)
        int sets = 0;
        bool validSets = false;
        while (!validSets)
        {
            Console.Write("Enter Number of Sets: ");
            string setsInput = Console.ReadLine();
            // Try to parse the string to an int (Chapter 5)
            if (int.TryParse(setsInput, out sets))
            {
                // Check if positive (Chapter 6)
                if (sets > 0)
                {
                    validSets = true;
                }
                else
                {
                    Console.WriteLine("Sets must be greater than 0.");
                }
            }
            else
            {
                Console.WriteLine("Invalid number. Please enter a whole number.");
            }
        }

        // Get Reps (with validation loop)
        int reps = 0;
        bool validReps = false;
        while (!validReps)
        {
            Console.Write("Enter Number of Reps: ");
            string repsInput = Console.ReadLine();
            if (int.TryParse(repsInput, out reps))
            {
                if (reps > 0)
                {
                    validReps = true;
                }
                else
                {
                    Console.WriteLine("Reps must be greater than 0.");
                }
            }
            else
            {
                Console.WriteLine("Invalid number. Please enter a whole number.");
            }
        }

        // Get Weight (with validation loop)
        double weight = 0.0;
        bool validWeight = false;
        while (!validWeight)
        {
            Console.Write("Enter Weight (lbs): ");
            string weightInput = Console.ReadLine();
            // Convert.ToDouble handles decimal numbers (Chapter 5)
            try
            {
                weight = Convert.ToDouble(weightInput);
                if (weight >= 0)
                {
                    validWeight = true;
                }
                else
                {
                    Console.WriteLine("Weight cannot be negative.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid format. Please enter a number (e.g., 135.5).");
            }
        }

        // Create the WorkoutSession object (Chapter 16, 18)
        WorkoutSession newSession = new WorkoutSession(date, name, sets, reps, weight);

        // Add to the List<T> (Chapter 20)
        workoutLog.Add(newSession);

        Console.WriteLine("Workout added successfully to memory!");
    }

    // =============================================
    // METHOD: ViewWorkouts
    // Displays all workouts currently in memory.
    // =============================================
    public static void ViewWorkouts()
    {
        Console.WriteLine("\n--- Current Workouts in Memory ---");

        // Check if the list is empty (Chapter 6)
        if (workoutLog.Count == 0)
        {
            Console.WriteLine("No workouts in memory. Add some or load from file.");
            return; // Exit the method early
        }

        // Iterate over the list using foreach (Chapter 12)
        // Note: We are allowed foreach on List<T>.
        foreach (WorkoutSession session in workoutLog)
        {
            // Call the Display method defined in the WorkoutSession class
            session.Display();
        }
    }

    // =============================================
    // METHOD: SaveToFile
    // Saves the in-memory list to a text file using StreamWriter.
    // =============================================
    public static void SaveToFile()
    {
        Console.WriteLine("\n--- Saving to File ---");

        // Check if there is data to save
        if (workoutLog.Count == 0)
        {
            Console.WriteLine("No data in memory to save.");
            return;
        }

        // We use a try-catch block for basic error handling.
        // This isn't strictly required by the allowed chapters, but it's
        // essential for file I/O to prevent crashes if the file is locked.
        try
        {
            // StreamWriter (Chapter 24)
            // 'using' statement ensures the file is closed properly, even if errors occur.
            // This is a best practice for File I/O.
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                Console.WriteLine($"Writing to {filePath}...");

                // Loop through each workout in the list (Chapter 12)
                foreach (WorkoutSession session in workoutLog)
                {
                    // Convert the object to a pipe-delimited string
                    string line = session.ToFileFormat();

                    // Write the line to the file
                    writer.WriteLine(line);
                }
            }

            Console.WriteLine($"Successfully saved {workoutLog.Count} workouts to '{filePath}'.");
        }
        catch (Exception ex)
        {
            // If something goes wrong (e.g., disk full, permissions), show error.
            Console.WriteLine($"Error saving file: {ex.Message}");
        }
    }

    // =============================================
    // METHOD: LoadFromFile
    // Loads data from the text file into memory using StreamReader.
    // =============================================
    public static void LoadFromFile()
    {
        Console.WriteLine("\n--- Loading from File ---");

        // Check if file exists before trying to read (Chapter 24)
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File '{filePath}' does not exist. Nothing to load.");
            return;
        }

        try
        {
            // Clear existing data in memory first?
            // Let's ask the user to avoid accidental overwrites.
            if (workoutLog.Count > 0)
            {
                Console.Write("Memory already contains data. Clear it before loading? (y/n): ");
                string response = Console.ReadLine();
                if (response.ToLower() == "y")
                {
                    workoutLog.Clear();
                    Console.WriteLine("Memory cleared.");
                }
                else
                {
                    Console.WriteLine("Load cancelled.");
                    return;
                }
            }

            // StreamReader (Chapter 24)
            // 'using' statement ensures the file is closed properly.
            using (StreamReader reader = new StreamReader(filePath))
            {
                Console.WriteLine($"Reading from {filePath}...");

                string line;
                int lineCount = 0;

                // ReadLine returns null when the end of the file is reached (Chapter 8)
                while ((line = reader.ReadLine()) != null)
                {
                    lineCount++;

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue; // Skip to next iteration (Chapter 10)
                    }

                    // PARSE THE LINE (Chapter 23)
                    // We expect format: Date|Exercise|Sets|Reps|Weight
                    string[] parts = line.Split('|');

                    // Validate the format (we expect 5 parts)
                    if (parts.Length == 5)
                    {
                        try
                        {
                            // Extract parts (Arrays, Indexing - Chapter 11)
                            string date = parts[0];
                            string name = parts[1];

                            // Convert strings to numbers (Chapter 5)
                            int sets = int.Parse(parts[2]);
                            int reps = int.Parse(parts[3]);
                            double weight = double.Parse(parts[4]);

                            // Create object and add to list
                            WorkoutSession loadedSession = new WorkoutSession(date, name, sets, reps, weight);
                            workoutLog.Add(loadedSession);
                        }
                        catch (FormatException)
                        {
                            // Handle cases where numbers are malformed in the file
                            Console.WriteLine($"Warning: Skipping line {lineCount} due to format error.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Skipping line {lineCount} (incorrect number of columns).");
                    }
                }
            }

            Console.WriteLine($"Loaded {workoutLog.Count} workouts into memory.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file: {ex.Message}");
        }
    }

    // =============================================
    // METHOD: ClearMemory
    // Clears the list from memory.
    // =============================================
    public static void ClearMemory()
    {
        workoutLog.Clear();
        Console.WriteLine("\nMemory cleared. No workouts in memory.");
    }
}
