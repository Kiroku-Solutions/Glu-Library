using Glu_Library.Models;
using Glu_Library.Services.Interfaces;

namespace Glu_Library.Services;

public class TranscriptStateManager : ITranscriptState
{
    private readonly List<SpeakerSegment> _segments = new();

    public IReadOnlyList<SpeakerSegment> Segments => _segments;
    public TranscriptResult? CurrentPartial { get; private set; }

    public event Action? OnStateChanged;

    public void ProcessResult(TranscriptResult result)
    {
        if (result.IsFinal)
        {
            AddFinalSegment(result);
            // Clear partial state since the text has been confirmed/finalized
            CurrentPartial = null; 
        }
        else
        {
            // Update the transient/partial view
            CurrentPartial = result;
        }

        NotifyStateChanged();
    }

    private void AddFinalSegment(TranscriptResult result)
    {
        var speaker = result.Speaker ?? "Unknown";
        var lastSegment = _segments.LastOrDefault();

        // Basic diarization logic: Merge if same speaker
        if (lastSegment != null && lastSegment.SpeakerId == speaker)
        {
            lastSegment.Text = $"{lastSegment.Text}{result.Text}";
            lastSegment.EndTimeMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
        }
        else
        {
            var timestamp = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
            _segments.Add(new SpeakerSegment
            {
                SpeakerId = speaker,
                Text = result.Text,
                // --- CAMBIO CLAVE AQUÍ ---
                IsAgent = IdentifyAgent(speaker), 
                StartTimeMs = timestamp,
                EndTimeMs = timestamp
            });
        }
    }

    public void Reset()
    {
        _segments.Clear();
        CurrentPartial = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    /// <summary>
    /// Lógica para separar visualmente a los hablantes.
    /// Basado en tus logs: "1" y "2".
    /// </summary>
    private bool IdentifyAgent(string speakerId)
    {
        // Asumimos que el "2" es el Médico/Agente (Azul)
        // y el "1" es el Paciente/Usuario (Blanco).
        // Si salen más hablantes, puedes ajustar esta lógica.
        return speakerId == "2";
    }
}