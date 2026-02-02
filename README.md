# Glu-Library

## Descripción

Esta librería de .NET (C#) está diseñada para interactuar con los servicios de Soniox a través de WebSockets, gestionando la transcripción, traducción y el estado de los segmentos de hablantes. Proporciona una interfaz para la comunicación en tiempo real con el API de Soniox, así como la gestión del estado de la transcripción.

## Instalación

(Próximamente: Detalles sobre cómo instalar esta librería como un paquete NuGet o referencia de proyecto.)

## Uso

La librería ofrece clientes de WebSocket y gestores de estado para facilitar la integración con los servicios de Soniox. Aquí hay un ejemplo básico de cómo podrías usar el `SonioxWebSocketClient`:

```csharp
// Ejemplo de uso básico (placeholder)
// var client = new SonioxWebSocketClient(...);
// client.ConnectAsync(...);
// client.StartTranscriptionAsync(...);
// client.OnTranscriptReceived += (sender, args) => { /* manejar transcripción */ };
```

## Configuración

La configuración para la conexión a Soniox se gestiona a través de la clase `SonioxWebSocketOptions`.

```csharp
// Ejemplo de configuración (placeholder)
// services.Configure<SonioxWebSocketOptions>(Configuration.GetSection("SonioxWebSocket"));
```

## Modelos

Los modelos clave incluyen:
- `SpeakerSegment.cs`: Representa segmentos de habla asociados a un hablante.
- `TranscriptResult.cs`: Contiene el resultado completo de una transcripción.
- Modelos `WebSocket/`: Clases para la comunicación WebSocket con Soniox (ej. `SonioxStartRequest`, `SonioxStreamResponse`, `SonioxErrorResponse`, `SonioxContext`, `SonioxTranslationConfig`).

## Servicios

Los servicios principales son:
- `ISonioxWebSocketClient.cs` / `SonioxWebSocketClient.cs`: Maneja la conexión y comunicación con el WebSocket de Soniox.
- `ITranscriptState.cs` / `TranscriptStateManager.cs`: Gestiona el estado de la transcripción, incluyendo segmentos de hablantes y resultados.

## Contribución

(Próximamente: Guías para contribuir al proyecto.)

## Licencia

(Próximamente: Información sobre la licencia del proyecto.)
