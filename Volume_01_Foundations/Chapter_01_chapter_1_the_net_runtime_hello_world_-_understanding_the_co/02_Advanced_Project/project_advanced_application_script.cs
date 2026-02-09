
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

namespace CoffeeShopReport
{
    class Program
    {
        static void Main(string[] args)
        {
            // =============================================================
            // SECTION 1: HEADER
            // =============================================================
            // We start by printing the title of the report and the date.
            // We use empty WriteLine() calls to create vertical spacing.
            
            Console.WriteLine("========================================");
            Console.WriteLine("   DAILY COFFEE SHOP OPERATIONS REPORT  ");
            Console.WriteLine("========================================");
            Console.WriteLine(); // Blank line for readability
            Console.WriteLine("Date: October 27, 2023");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine();

            // =============================================================
            // SECTION 2: HOURS OF OPERATION
            // =============================================================
            // Here we display the opening and closing times.
            // Note: We are not calculating duration; we are simply reporting
            // the recorded times as string literals.
            
            Console.WriteLine("HOURS OF OPERATION:");
            Console.WriteLine("  Opening Time: 7:00 AM");
            Console.WriteLine("  Closing Time: 5:00 PM");
            Console.WriteLine(); // Blank line

            // =============================================================
            // SECTION 3: STAFFING
            // =============================================================
            // We identify the staff member who was responsible for the shift.
            
            Console.WriteLine("STAFF ON DUTY:");
            Console.WriteLine("  Primary Barista: Alice");
            Console.WriteLine();

            // =============================================================
            // SECTION 4: SALES METRICS
            // =============================================================
            // We report the raw sales numbers.
            // We use string concatenation (joining text together) to form
            // a complete sentence within the WriteLine method.
            
            Console.WriteLine("SALES METRICS:");
            Console.WriteLine("  Total Cups Sold: 142");
            Console.WriteLine("  Best-Selling Item: Caramel Macchiato");
            Console.WriteLine();

            // =============================================================
            // SECTION 5: INCIDENT LOG
            // =============================================================
            // We print the specific incident reported during the shift.
            // We use indentation (spaces) to align the text visually.
            
            Console.WriteLine("INCIDENT LOG:");
            Console.WriteLine("  10:15 AM - Minor spill near the register.");
            Console.WriteLine("             Cleaned immediately.");
            Console.WriteLine();

            // =============================================================
            // SECTION 6: FOOTER
            // =============================================================
            // We conclude the report with a standard closing message.
            
            Console.WriteLine("========================================");
            Console.WriteLine("   END OF REPORT   ");
            Console.WriteLine("========================================");
        }
    }
}
