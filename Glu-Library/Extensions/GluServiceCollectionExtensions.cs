using Glu_Library.Configuration;
using Glu_Library.Services;
using Glu_Library.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Glu_Library.Extensions;

public static class GluServiceCollectionExtensions
{
    /// <summary>
    /// Registers Glu (Soniox) services into the dependency injection container.
    /// This extension method simplifies the setup process for the consuming application.
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <param name="configuration">The configuration provider (e.g., appsettings.json) used to retrieve the API Key and options.</param>
    /// <returns>The updated IServiceCollection for method chaining.</returns>
    public static IServiceCollection AddGlu(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Bind the configuration section (e.g., "Soniox") to the strongly-typed SonioxOptions class.
        // This allows services to inject IOptions<SonioxOptions> cleanly.
        var section = configuration.GetSection(SonioxOptions.SectionName);
        services.Configure<SonioxOptions>(section);

        // 2. Register the core Token Service using the HttpClient Factory pattern.
        // This automatically injects an optimized and managed HttpClient instance into SonioxTokenService.
        services.AddHttpClient<ISonioxTokenService, SonioxTokenService>();

        // 3. Register the Transcript State Manager.
        // We use Scoped lifetime so the conversation state persists for the duration of a client request or SignalR connection,
        // but is isolated between different user sessions.
        services.AddScoped<ITranscriptState, TranscriptStateManager>();

        return services;
    }
}