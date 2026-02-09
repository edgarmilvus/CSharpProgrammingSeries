
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

public class LocationStat
{
    public string Location { get; set; }
    public double AverageValue { get; set; }
    public int ReadingCount { get; set; }
}

public void ExecuteExercise2()
{
    var source = DataSource.GetReadings();

    var locationStats = 
        from reading in source
        where reading.IsValid // Filter before grouping
        group reading by reading.Location into locationGroup
        where locationGroup.Count() >= 2 // Filter the groups
        select new LocationStat
        {
            Location = locationGroup.Key,
            AverageValue = locationGroup.Average(r => r.Value),
            ReadingCount = locationGroup.Count()
        };

    // Execution
    foreach (var stat in locationStats)
    {
        Console.WriteLine($"Location: {stat.Location}, Avg: {stat.AverageValue:F2}, Count: {stat.ReadingCount}");
    }
}
