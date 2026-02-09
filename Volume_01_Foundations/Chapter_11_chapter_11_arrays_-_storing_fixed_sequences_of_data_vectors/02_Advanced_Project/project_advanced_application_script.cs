
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

using System;

class PhotoAlbum
{
    static void Main()
    {
        // --- PART 1: INITIALIZATION ---
        // We declare an array of strings to hold exactly 5 photo filenames.
        // We initialize it with some default values.
        string[] photos = new string[5];
        photos[0] = "sunset.jpg";
        photos[1] = "cat.png";
        photos[2] = "mountain.bmp";
        photos[3] = "street.jpg";
        photos[4] = "river.png";

        // --- PART 2: VIEWING THE ALBUM ---
        // We display the current contents of the album to the user.
        Console.WriteLine("--- Current Photo Album ---");
        
        // We use a for loop to iterate through every index of the array.
        for (int i = 0; i < 5; i++)
        {
            // Check if the slot is empty (null) or contains a filename.
            if (photos[i] == null)
            {
                Console.WriteLine($"Slot {i + 1}: [Empty]");
            }
            else
            {
                Console.WriteLine($"Slot {i + 1}: {photos[i]}");
            }
        }
        Console.WriteLine(); // Empty line for spacing.

        // --- PART 3: UPDATING A PHOTO ---
        // Let's say we want to replace the blurry photo in slot 3 (index 2).
        Console.WriteLine("--- Updating Slot 3 ---");
        Console.WriteLine($"Old photo: {photos[2]}");
        
        // We assign a new value to the specific index.
        photos[2] = "alpine_peak.jpg";
        
        Console.WriteLine($"New photo: {photos[2]}");
        Console.WriteLine("Update complete.");
        Console.WriteLine();

        // --- PART 4: SEARCHING THE ALBUM ---
        // We want to check if "cat.png" is in the album.
        Console.WriteLine("--- Searching for 'cat.png' ---");
        
        // We use a boolean flag to track if we found the photo.
        bool found = false;
        
        // We loop through the array again to check every slot.
        for (int i = 0; i < 5; i++)
        {
            // We compare the current photo with the target name.
            // We use '==' for string comparison.
            if (photos[i] == "cat.png")
            {
                Console.WriteLine($"Found 'cat.png' at Slot {i + 1}.");
                found = true;
                // Since we found it, we can stop searching.
                break; 
            }
        }

        // If the loop finishes and found is still false, the photo wasn't there.
        if (!found)
        {
            Console.WriteLine("The photo 'cat.png' was not found in the album.");
        }
        
        Console.WriteLine();
        
        // --- PART 5: EDGE CASE (Index Out of Bounds) ---
        // Let's demonstrate what happens if we try to access an index that doesn't exist.
        Console.WriteLine("--- Testing Array Limits ---");
        Console.WriteLine("Attempting to access Slot 6 (Index 5)...");
        
        // The valid indices are 0, 1, 2, 3, 4.
        // Index 5 is outside the bounds of the array.
        // Uncommenting the line below would cause the program to crash.
        // string invalidPhoto = photos[5]; 
        
        Console.WriteLine("Note: Accessing index 5 would throw an exception.");
        Console.WriteLine("We must always ensure our index is within the range 0 to (Length - 1).");
        
        // We can check the length of the array safely.
        Console.WriteLine($"Total slots in album: {photos.Length}");
    }
}
