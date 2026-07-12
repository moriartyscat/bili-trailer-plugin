using Jellyfin.Plugin.BiliTrailers.Providers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.BiliTrailers;

/// <summary>
/// Registers the B站 trailer metadata provider with Jellyfin's DI container.
/// </summary>
public class ServiceRegistrator : IServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddHttpClient("BiliTrailers", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Jellyfin-BiliTrailers/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        serviceCollection.AddSingleton<BiliTrailerMetadataProvider>();

        // Register as a metadata provider for Movie items
        serviceCollection.AddSingleton<IMetadataProvider<MediaBrowser.Controller.Entities.Movies.Movie>>(
            sp => sp.GetRequiredService<BiliTrailerMetadataProvider>());

        serviceCollection.AddSingleton<IRemoteMetadataProvider<MediaBrowser.Controller.Entities.Movies.Movie, MediaBrowser.Model.Providers.MovieInfo>>(
            sp => sp.GetRequiredService<BiliTrailerMetadataProvider>());
    }
}
