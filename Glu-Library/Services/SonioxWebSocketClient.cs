using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization; // Necesario para las opciones de JSON
using Glu_Library.Configuration;
using Glu_Library.Models;
using Glu_Library.Models.WebSocket;
using Glu_Library.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Glu_Library.Services;

public sealed class SonioxWebSocketClient : ISonioxWebSocketClient
{
    private ClientWebSocket? _webSocket;
    
    // Configuración JSON robusta para evitar errores por mayúsculas/minúsculas
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly Uri _webSocketUri;
    private readonly SonioxStartRequest _startRequest; 
    private CancellationTokenSource? _receiveCts;

    public event Action<TranscriptResult>? OnTranscriptReceived;
    public event Action<Exception>? OnError;

    // Propiedad que se llenará desde el UI con la frecuencia real (48000/44100)
    public int SampleRate { get; set; } = 48000; 

    public SonioxWebSocketClient(IOptions<SonioxWebSocketOptions> options)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.Endpoint))
            throw new InvalidOperationException("Soniox Endpoint no configurado.");

        _webSocketUri = new Uri(opts.Endpoint);

        // --- CONFIGURACIÓN V3 ---
        _startRequest = new SonioxStartRequest
        {
            ApiKey = opts.Token,
            
            // Modelo V3 recomendado
            Model = "stt-rt-v3", 
            
            // Pistas de idioma (Español)
            LanguageHints = new List<string> { "es" },

            // Configuración de Audio requerida para RAW PCM
            AudioFormat = "pcm_s16le",
            NumChannels = 1,
            // SampleRate se asignará en ConnectAsync
            
            EnableGlobalSpeakerDiarization = opts.EnableSpeakerDiarization,
            
            // V3 maneja esto muy bien para detectar frases completas
            EnableEndpointDetection = true 
        };
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Limpieza preventiva
            if (_webSocket != null) _webSocket.Dispose();
            _webSocket = new ClientWebSocket();

            await _webSocket.ConnectAsync(_webSocketUri, cancellationToken);

            // ACTUALIZAR SAMPLE RATE (Vital)
            _startRequest.SampleRate = this.SampleRate;

            await SendJsonAsync(_startRequest, cancellationToken);

            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = ReceiveLoopAsync(_receiveCts.Token);
        }
        catch (Exception ex)
        {
            RaiseError(ex);
            throw;
        }
    }

    public async Task SendAudioAsync(ReadOnlyMemory<byte> audioData, CancellationToken cancellationToken = default)
    {
        if (_webSocket is null || _webSocket.State != WebSocketState.Open) return;

        try
        {
            await _webSocket.SendAsync(audioData, WebSocketMessageType.Binary, true, cancellationToken);
        }
        catch (Exception ex)
        {
            RaiseError(ex);
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            _receiveCts?.Cancel();
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            RaiseError(ex);
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];

        try
        {
            Console.WriteLine("🟢 [SonioxClient] Conectado. Esperando audio...");

            while (!cancellationToken.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"🔴 [SonioxClient] Servidor cerró: {result.CloseStatusDescription}");
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // Console.WriteLine($"RAW: {json}"); // Descomentar para debug
                    ProcessIncomingMessage(json);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 [SonioxClient] Error loop: {ex.Message}");
            RaiseError(ex);
        }
    }

    // --- LÓGICA V3 (Actualizada para leer 'tokens' en vez de 'fw') ---
    private void ProcessIncomingMessage(string json)
    {
        try
        {
            var response = JsonSerializer.Deserialize<SonioxStreamResponse>(json, _jsonOptions);
            
            // Validar errores
            if (response?.ErrorCode != null)
            {
                Console.WriteLine($"❌ Error Soniox ({response.ErrorCode}): {response.ErrorMessage}");
                return;
            }

            if (response?.Tokens == null || response.Tokens.Count == 0) return;

            // --- A) Tokens Finales (Confirmados) ---
            // Soniox V3 envía los finales con is_final = true
            var finalTokens = response.Tokens.Where(t => t.IsFinal).ToList();
            if (finalTokens.Count > 0)
            {
                // Usamos Join vacio "" porque Soniox V3 incluye los espacios en el token (ej: " hello")
                var text = string.Join("", finalTokens.Select(t => t.Text));
                var speaker = finalTokens.First().Speaker ?? "0";

                Console.WriteLine($"✅ FINAL: {text}");

                OnTranscriptReceived?.Invoke(new TranscriptResult
                {
                    Text = text,
                    IsFinal = true,
                    Speaker = speaker,
                    Timestamp = DateTime.UtcNow
                });
            }

            // --- B) Tokens Parciales (Borrador) ---
            // Soniox V3 envía hipótesis con is_final = false
            var nonFinalTokens = response.Tokens.Where(t => !t.IsFinal).ToList();
            if (nonFinalTokens.Count > 0)
            {
                var text = string.Join("", nonFinalTokens.Select(t => t.Text));
                
                // Console.WriteLine($"... Parcial: {text}");

                OnTranscriptReceived?.Invoke(new TranscriptResult
                {
                    Text = text,
                    IsFinal = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parseando JSON: {ex.Message}");
        }
    }

    private async Task SendJsonAsync<T>(T payload, CancellationToken ct)
    {
        if (_webSocket is null) return;
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        
        Console.WriteLine($"📤 [SonioxClient] Config: {json}");
        
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    private void RaiseError(Exception ex) => OnError?.Invoke(ex);

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _webSocket?.Dispose();
        _receiveCts?.Dispose();
    }
}