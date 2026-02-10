
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;

public class VideoGameCharacter
{
    public string Name { get; }
    public int Health { get; }
    public int Level { get; }
    public string CharacterClass { get; }

    // Primary Constructor (The "Master" definition)
    public VideoGameCharacter(string name, int health, int level, string characterClass)
    {
        Name = name;
        Health = health;
        Level = level;
        CharacterClass = characterClass;
    }

    // Secondary Constructor (Chained)
    // The ": this(...)" syntax calls the Primary Constructor
    public VideoGameCharacter(string name, string characterClass) 
        : this(name, 100, 1, characterClass) 
    {
        // The body is empty because 'this' handles all the assignment.
        // We could add extra logic here if needed, but assignment is done by the primary.
    }

    public void DisplayStats()
    {
        Console.WriteLine($"Name: {Name} | Class: {CharacterClass} | Lvl: {Level} | HP: {Health}");
    }
}

public class Program
{
    public static void Main()
    {
        // Create a level 5 warrior with 150 HP (Uses Primary Constructor)
        VideoGameCharacter warrior = new VideoGameCharacter("Conan", 150, 5, "Warrior");
        
        // Create a default mage (Uses Secondary Constructor)
        // This automatically passes 100 HP and Level 1
        VideoGameCharacter mage = new VideoGameCharacter("Gandalf", "Mage");

        warrior.DisplayStats();
        mage.DisplayStats();
    }
}
