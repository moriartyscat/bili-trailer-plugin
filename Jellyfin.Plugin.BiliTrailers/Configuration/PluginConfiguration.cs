using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.BiliTrailers.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        ServiceBaseUrl = "http://localhost:8686";
        MaxTrailersPerMovie = 3;
        AutoMatch = true;
    }

    /// <summary>
    /// Gets or sets the base URL of the bili-service companion.
    /// </summary>
    public string ServiceBaseUrl { get; set; } = "http://localhost:8686";

    /// <summary>
    /// Gets or sets the maximum number of B站 trailers to attach per movie.
    /// </summary>
    public int MaxTrailersPerMovie { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to auto-search for trailers during metadata refresh.
    /// </summary>
    public bool AutoMatch { get; set; } = true;
}
