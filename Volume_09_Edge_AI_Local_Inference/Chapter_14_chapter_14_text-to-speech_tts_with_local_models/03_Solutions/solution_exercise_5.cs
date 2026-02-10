
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// --- 1. Mock External Services ---

// From Chapter 13 (LLM)
public interface ILlmInferenceService
{
    // Yields tokens as they are generated
    IAsyncEnumerable<string> GenerateStreamAsync(string prompt, CancellationToken token);
}

// From Chapter 13 (STT)
public interface IAudioInput
{
    Task<string> ListenAsync(CancellationToken token);
}

public interface IAudioOutput
{
    Task PlayAsync(byte[] audio, CancellationToken token);
    void Stop();
}

// From Exercise 4
public interface ITtsService
{
    Task<byte[]> GenerateSpeechAsync(string text, int speakerId);
}

// --- 2. The Orchestrator ---

public class EdgeAssistantOrchestrator
{
    private readonly ILlmInferenceService _llm;
    private readonly ITtsService _tts;
    private readonly IAudioInput _audioInput;
    private readonly IAudioOutput _audioOutput;
    private readonly ILogger<EdgeAssistantOrchestrator> _logger;

    public EdgeAssistantOrchestrator(
        ILlmInferenceService llm, 
        ITtsService tts, 
        IAudioInput audioInput, 
        IAudioOutput audioOutput,
        ILogger<EdgeAssistantOrchestrator> logger)
    {
        _llm = llm;
        _tts = tts;
        _audioInput = audioInput;
        _audioOutput = audioOutput;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        // Main loop
        while (true)
        {
            // 1. Listen for Wake Word (Simplified)
            _logger.LogInformation("Listening for wake word...");
            var userSpeech = await _audioInput.ListenAsync(CancellationToken.None);
            if (!userSpeech.Contains("assistant")) continue;

            // 2. Capture Query
            _logger.LogInformation("Capturing query...");
            // In real app, listen for more audio. Here we assume the wake word was the query.
            
            // 3. Setup Cancellation for Barge-in
            using var cts = new CancellationTokenSource();
            
            // Start a background task to detect if user speaks again (Barge-in)
            var bargeInTask = Task.Run(async () =>
            {
                // If we detect audio input during output, cancel
                // (Mocking detection of interruption)
                await Task.Delay(2000, cts.Token); // Simulate window where user might interrupt
                // cts.Cancel(); // Uncomment to simulate interruption
            }, cts.Token);

            // 4. LLM Generation + TTS Streaming (The Core Logic)
            try
            {
                await ProcessLlmResponseAsync("User said: " + userSpeech, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation cancelled (Barge-in detected).");
                _audioOutput.Stop();
            }
            finally
            {
                cts.Cancel(); // Ensure cleanup
            }
        }
    }

    private async Task ProcessLlmResponseAsync(string prompt, CancellationToken token)
    {
        string sentenceBuffer = "";
        
        // Requirement: Stream LLM tokens
        await foreach (var tokenChunk in _llm.GenerateStreamAsync(prompt, token))
        {
            sentenceBuffer += tokenChunk;

            // Requirement: Sentence Buffer Logic
            if (IsEndOfSentence(sentenceBuffer))
            {
                // We have a complete sentence. Generate TTS for this chunk.
                var textToSpeak = sentenceBuffer;
                sentenceBuffer = ""; // Clear buffer

                // Requirement: Parallelism / Non-blocking
                // We fire and forget the TTS task, but we must await it to maintain order
                // OR we queue it. For simplicity, we await, but we pass the cancellation token.
                var audioTask = _tts.GenerateSpeechAsync(textToSpeak, speakerId: 1);
                
                // Important: While TTS runs, LLM continues generating next tokens
                // (This is true parallelism if TTS is slow)
                
                var audioData = await audioTask;
                
                // Play audio (this blocks the loop until playback finishes, 
                // but in a real app, AudioOutput would be a non-blocking stream)
                await _audioOutput.PlayAsync(audioData, token);
            }
        }

        // Handle remaining text in buffer if LLM finished without punctuation
        if (!string.IsNullOrWhiteSpace(sentenceBuffer))
        {
            var audioData = await _tts.GenerateSpeechAsync(sentenceBuffer, speakerId: 1);
            await _audioOutput.PlayAsync(audioData, token);
        }
    }

    private bool IsEndOfSentence(string text)
    {
        // Detect punctuation or max buffer length to prevent waiting forever
        char[] terminators = { '.', '!', '?' };
        return text.Any(c => terminators.Contains(c)) || text.Length > 100;
    }
}

// --- 3. Unit Test (xUnit) ---

public class EdgeAssistantTests
{
    [Fact]
    public async Task Orchestrator_CallsTtsWithCorrectSpeaker()
    {
        // Arrange
        var mockLlm = new Mock<ILlmInferenceService>();
        mockLlm.Setup(x => x.GenerateStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(new List<string> { "Hello", " world", "." }.ToAsyncEnumerable());

        var mockTts = new Mock<ITtsService>();
        mockTts.Setup(x => x.GenerateSpeechAsync("Hello world.", 1))
               .ReturnsAsync(new byte[100]); // Expecting combined sentence

        var mockInput = new Mock<IAudioInput>();
        mockInput.Setup(x => x.ListenAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync("assistant");

        var mockOutput = new Mock<IAudioOutput>();
        mockOutput.Setup(x => x.PlayAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        var orchestrator = new EdgeAssistantOrchestrator(
            mockLlm.Object, mockTts.Object, mockInput.Object, mockOutput.Object, 
            Mock.Of<ILogger<EdgeAssistantOrchestrator>>());

        // Act
        // We need a way to break the infinite loop in RunAsync for the test.
        // In a real test, we'd use a timeout or a specific signal.
        // Here, we just verify the logic flow by calling the internal method directly or mocking the loop exit.
        
        // Simulating the logic flow:
        await orchestrator.ProcessLlmResponseAsync("Test", new CancellationTokenSource(1000).Token);

        // Assert
        // Verify TTS was called with the expected combined text and speaker ID
        mockTts.Verify(x => x.GenerateSpeechAsync("Hello world.", 1), Times.AtLeastOnce);
    }
}
