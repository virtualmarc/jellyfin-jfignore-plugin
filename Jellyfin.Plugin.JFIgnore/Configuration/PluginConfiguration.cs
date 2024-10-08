using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.JFIgnore.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Default Filename.
    /// </summary>
    public const string FILENAME = ".jfignore";

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // set default options here
        IgnoreFilename = FILENAME;
    }

    /// <summary>
    /// Gets or sets a value of the patterns we want to ignore.
    /// </summary>
    public string IgnoreFilename { get; set; }
}
