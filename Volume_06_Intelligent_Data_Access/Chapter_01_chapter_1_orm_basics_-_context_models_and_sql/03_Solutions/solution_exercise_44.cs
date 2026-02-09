
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

# Source File: solution_exercise_44.cs
# Description: Solution for Exercise 44
# ==========================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Knowledge Graph + Vectors
public class EducationService
{
    private readonly IVectorRepository<Lesson> _lessonRepo;

    public async Task<List<Lesson>> GetLearningPathAsync(string studentId, string topic)
    {
        // 1. Get Student Knowledge Vector
        // Represents what the student knows (based on quiz scores, time spent)
        var studentVector = await GetStudentVectorAsync(studentId);

        // 2. Get Topic Vector
        var topicVector = await GenerateEmbedding(topic);

        // 3. Search: Find lessons that bridge the gap
        // We want lessons that are slightly more advanced than the student's current vector
        // but aligned with the topic vector.
        
        var lessons = await _lessonRepo.SearchAsync(topicVector, 20);

        return lessons
            .Where(l => IsAppropriateDifficulty(l, studentVector))
            .OrderBy(l => l.ComplexityScore)
            .ToList();
    }

    private bool IsAppropriateDifficulty(Lesson lesson, float[] studentVector)
    {
        // Calculate "Knowledge Gap"
        // If the lesson vector is too far from student vector, it's too hard.
        // If too close, it's too easy.
        return true; // Mock
    }
}
