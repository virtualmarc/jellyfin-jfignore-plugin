using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using Jellyfin.Plugin.JFIgnore.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JFIgnore;

/// <inheritdoc />
public class JFIgnoreFileRule : IResolverIgnoreRule
{
    private readonly ILogger<JFIgnoreFileRule> _logger;

    private static Dictionary<string, Ignore.Ignore> _ignoreCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="JFIgnoreFileRule"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public JFIgnoreFileRule(ILogger<JFIgnoreFileRule> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Clear the ignore cache.
    /// </summary>
    public static void ClearIgnoreCache()
    {
        _ignoreCache.Clear();
    }

    /// <inheritdoc />
    public bool ShouldIgnore(FileSystemMetadata fileInfo, BaseItem? parent)
    {
        string ignoreFilename = Plugin.Instance?.Configuration.IgnoreFilename ?? PluginConfiguration.FILENAME;
        if (ignoreFilename.Length == 0)
        {
            return false;
        }

        _logger.LogInformation("FileName: {FileName}, Extension: {FileExtension}, FullName: {FullName}, IsDirectory: {IsDirectory}, ParentName: {ParentName}, ParentPath: {ParentPath}, TopParentPath: {TopParentPath}", fileInfo.Name, fileInfo.Extension, fileInfo.FullName, fileInfo.IsDirectory, parent?.Name, parent?.Path, parent?.GetTopParent().Path);

        return false;
    }
}
