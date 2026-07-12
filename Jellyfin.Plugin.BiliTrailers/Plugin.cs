using System;
using System.Collections.Generic;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.BiliTrailers;

/// <summary>
/// BiliTrailers plugin: adds B站 (Bilibili) trailer URLs to movies during metadata refresh.
/// Requires the bili-service companion to be running alongside Jellyfin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <inheritdoc />
    public override string Name => "B站 Trailers";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("a4b2c8d0-1e2f-3a4b-5c6d-7e8f9a0b1c2d");

    /// <inheritdoc />
    public override string Description => "Add Bilibili trailers to your movie library. "
        + "Requires bili-service companion (default http://localhost:8686).";

    /// <inheritdoc />
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets the companion service base URL.
    /// </summary>
    public string ServiceBaseUrl => Configuration.ServiceBaseUrl;

    /// <inheritdoc />
    public IEnumerable<PageInfo> GetPages()
    {
        yield return new PageInfo
        {
            Name = "B站 Trailers",
            DisplayName = "B站 Trailers",
            EnableInMainMenu = true,
            MenuSection = "server",
            MenuIcon = "ondemand_video",
            EmbedResourcePath = GetType().Namespace + ".Configuration.configPage.html",
        };
    }
}
