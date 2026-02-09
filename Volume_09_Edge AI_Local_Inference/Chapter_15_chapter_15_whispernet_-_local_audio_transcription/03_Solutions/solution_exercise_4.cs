
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console; // Using Spectre.Console for rich CLI output

// Assuming the existing interface from Exercise 2
public interface IAdvancedRealTimeTranscriber : IRealTimeAudioTranscriber
{
    Task<string> DetectLanguageAsync(CancellationToken cancellationToken = default);
    IObservable<SpeakerSegment> GetSpeakerSegments();
    Task SaveSessionAsync(string sessionPath, CancellationToken cancellationToken = default);
    Task LoadSessionAsync(string sessionPath, CancellationToken cancellationToken = default);
    bool EnableSpeakerDiarization { get; set; }
    string PreferredLanguage { get; set; }
}

public class AdvancedRealTimeTranscriber : IAdvancedRealTimeTranscriber
{
    // State variables
    public bool EnableSpeakerDiarization { get; set; } = false;
    public string PreferredLanguage { get; set; } = "en";
    
    // Dependencies (Mocked for solution brevity)
    private readonly NoiseReductionFilter _noiseFilter = new();
    private readonly SpeakerDiarizationService _diarizationService = new();
    private readonly List<TranscriptionSegment> _sessionLog = new();
    
    // Dashboard state
    private bool _isPaused = false;
    private string _currentLanguage = "Unknown";
    private double _currentVolume = 0;

    public async IAsyncEnumerable<TranscriptionSegment> StartTranscriptionAsync(string deviceName = null, CancellationToken cancellationToken = default)
    {
        // Start CLI Dashboard Task
        var dashboardTask = Task.Run(() => RunDashboardAsync(cancellationToken), cancellationToken);

        // Simulate audio processing loop
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_isPaused)
            {
                await Task.Delay(100, cancellationToken);
                continue;
            }

            // 1. Capture Audio (Mocked)
            var audioChunk = await CaptureAudioChunkAsync(cancellationToken);
            
            // 2. Energy-based Noise Reduction
            var cleanedAudio = _noiseFilter.Process(audioChunk);
            _currentVolume = CalculateRms(cleanedAudio);

            // 3. Language Detection (Simulated on first few chunks)
            if (_sessionLog.Count < 5)
            {
                _currentLanguage = await DetectLanguageAsync(cancellationToken);
            }

            // 4. Transcription & Speaker Diarization
            var segment = new TranscriptionSegment 
            { 
                Text = $"Processed audio in {_currentLanguage}", 
                StartTime = DateTime.Now,
                SpeakerId = EnableSpeakerDiarization ? await _diarizationService.IdentifySpeakerAsync(cleanedAudio) : "N/A"
            };

            _sessionLog.Add(segment);
            yield return segment;

            // Keyboard Shortcuts Check
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.P) _isPaused = true;
                if (key == ConsoleKey.R) _isPaused = false;
            }
        }
    }

    public Task<string> DetectLanguageAsync(CancellationToken cancellationToken = default)
    {
        // In real impl, use Whisper's language detection probability
        // Simulating detection logic
        var languages = new[] { "en", "es", "fr", "de" };
        var detected = languages[new Random().Next(languages.Length)];
        return Task.FromResult(detected);
    }

    public IObservable<SpeakerSegment> GetSpeakerSegments()
    {
        // Implementation of IObservable pattern
        return new Subject<SpeakerSegment>(); // Simplified
    }

    public async Task SaveSessionAsync(string sessionPath, CancellationToken cancellationToken = default)
    {
        // Serialize _sessionLog to JSON
        var json = System.Text.Json.JsonSerializer.Serialize(_sessionLog);
        await File.WriteAllTextAsync(sessionPath, json, cancellationToken);
    }

    public Task LoadSessionAsync(string sessionPath, CancellationToken cancellationToken = default)
    {
        // Load and deserialize
        throw new NotImplementedException();
    }

    // --- Dashboard Implementation ---
    private async Task RunDashboardAsync(CancellationToken token)
    {
        AnsiConsole.Clear();
        while (!token.IsCancellationRequested)
        {
            AnsiConsole.MarkupLine("[bold blue]Real-time Transcription Dashboard[/]");
            
            // Waveform Visualization (ASCII)
            AnsiConsole.Write(new Rule($"Volume: {_currentVolume:F2}") { RuleStyle = "grey" });
            DrawWaveform(_currentVolume);

            // Status Table
            var table = new Table();
            table.AddColumn("Metric");
            table.AddColumn("Value");
            table.AddRow("Language", _currentLanguage);
            table.AddRow("Diarization", EnableSpeakerDiarization ? "[green]ON[/]" : "[red]OFF[/]");
            table.AddRow("Status", _isPaused ? "[yellow]PAUSED[/]" : "[green]LIVE[/]");
            table.AddRow("Memory", $"{GC.GetTotalMemory(false) / 1024 / 1024} MB");
            AnsiConsole.Write(table);

            // Recent Transcriptions
            if (_sessionLog.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[bold]Recent Segments:[/]");
                foreach (var seg in _sessionLog.TakeLast(3))
                {
                    AnsiConsole.MarkupLine($"[{(_currentLanguage == "en" ? "green" : "yellow")}]{seg.SpeakerId}:[/] {seg.Text}");
                }
            }

            AnsiConsole.MarkupLine("\n[grey]Press 'P' to Pause, 'R' to Resume, 'L' to Toggle Language Display...[/]");
            
            await Task.Delay(500, token); // Update frequency
            AnsiConsole.Clear(); // Refresh screen
        }
    }

    private void DrawWaveform(double volume)
    {
        // Simple ASCII bar representing volume
        int bars = (int)(volume / 100 * 20);
        AnsiConsole.MarkupLine(new string('█', bars).PadRight(20));
    }

    // --- Helper Methods ---
    private async Task<byte[]> CaptureAudioChunkAsync(CancellationToken ct) => await Task.FromResult(new byte[1024]);
    private double CalculateRms(byte[] audio) => new Random().NextDouble() * 1000; // Simulated
}

// Placeholder classes
public class NoiseReductionFilter { public byte[] Process(byte[] audio) => audio; }
public class SpeakerDiarizationService { public Task<string> IdentifySpeakerAsync(byte[] audio) => Task.FromResult("Speaker A"); }
public class Subject<T> : IObservable<T> { public IDisposable Subscribe(IObserver<T> observer) => throw new NotImplementedException(); }
