using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.NoTranscode.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.NoTranscode;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "No Transcode";

    public override string Description =>
        "Снимает право на перекодирование у всех пользователей, оставляя только Direct Play.";

    // Уникальный GUID плагина. Если будешь форкать — сгенерируй новый (uuidgen).
    public override Guid Id => Guid.Parse("8f4a2c1e-5b3d-4e7a-9c2f-1d6e8b0a3f57");

    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = Name,
            EmbeddedResourcePath = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.Configuration.configPage.html",
                GetType().Namespace)
        };
    }
}
