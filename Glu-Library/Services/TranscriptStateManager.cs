using Glu_Library.Models;
using Glu_Library.Services.Interfaces;

namespace Glu_Library.Services;

/// <summary>
/// Gestiona el estado de la transcripción: segmentos finales y texto parcial en tiempo real.
/// </summary>
public class TranscriptStateManager : ITranscriptState
{
    // --- Estado Interno ---
    private readonly List<SpeakerSegment> _segments = new();

    public IReadOnlyList<SpeakerSegment> Segments => _segments;
    public TranscriptResult? CurrentPartial { get; private set; }

    public event Action? OnStateChanged;

    public void ProcessResult(TranscriptResult result)
    {
        // Si el resultado es final, lo guardamos en el historial
        if (result.IsFinal)
        {
            AddFinalSegment(result);
            CurrentPartial = null; // Limpiamos el parcial porque ya se confirmó
        }
        else
        {
            // Si es parcial, solo actualizamos la vista temporal
            CurrentPartial = result;
        }

        NotifyStateChanged();
    }

    private void AddFinalSegment(TranscriptResult result)
    {
        var speaker = result.Speaker ?? "Desconocido";
        var lastSegment = _segments.LastOrDefault();

        // Lógica simple de diarización: Si habla el mismo, unimos el texto
        if (lastSegment != null && lastSegment.SpeakerId == speaker)
        {
            lastSegment.Text = $"{lastSegment.Text}{result.Text}"; // Sin espacio extra, Soniox ya los trae
            lastSegment.EndTimeMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
        }
        else
        {
            // Nuevo hablante o primera frase
            var timestamp = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
            _segments.Add(new SpeakerSegment
            {
                SpeakerId = speaker,
                Text = result.Text,
                IsAgent = speaker == "1", // Ejemplo simple: Speaker 1 es agente
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
}