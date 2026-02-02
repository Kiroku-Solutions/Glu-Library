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

        // Register core services as Scoped.
        // This ensures that each user connection (or HTTP request) gets its own isolated instance
        // of the client and state manager, preventing data leaks between concurrent users.
        services.AddScoped<ITranscriptState, TranscriptStateManager>();
        services.AddScoped<ISonioxWebSocketClient, SonioxWebSocketClient>();

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

        // Register core services as Scoped to ensure isolation per user connection.
        services.AddScoped<ITranscriptState, TranscriptStateManager>();
        services.AddScoped<ISonioxWebSocketClient, SonioxWebSocketClient>();

        return services;
    }
}