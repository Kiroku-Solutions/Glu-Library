using Glu_Library.Configuration;
using Glu_Library.Services;
using Glu_Library.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Glu_Library.Extensions;

/// <summary>
/// Dependency injection extensions for registering Glu Soniox services.
/// This is the main entry point for consuming applications.
/// </summary>
public static class GluServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Soniox WebSocket transcription services using configuration binding.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (appsettings.json).</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGluSonioxWebSocket(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind and validate options
        services.Configure<SonioxWebSocketOptions>(
            configuration.GetSection(SonioxWebSocketOptions.SectionName));

        // Register core services
        services.AddSingleton<ITranscriptState, TranscriptStateManager>();
        services.AddSingleton<ISonioxWebSocketClient, SonioxWebSocketClient>();

        return services;
    }

    /// <summary>
    /// Registers the Soniox WebSocket transcription services using explicit options.
    /// Useful for tests or programmatic configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGluSonioxWebSocket(
        this IServiceCollection services,
        Action<SonioxWebSocketOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddSingleton<ITranscriptState, TranscriptStateManager>();
        services.AddSingleton<ISonioxWebSocketClient, SonioxWebSocketClient>();

        return services;
    }
}
