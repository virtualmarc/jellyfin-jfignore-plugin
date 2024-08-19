using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="JFIgnoreFileRule"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public JFIgnoreFileRule(ILogger<JFIgnoreFileRule> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool ShouldIgnore(FileSystemMetadata fileInfo, BaseItem? parent)
    {
        string ignoreFilename = Plugin.Instance?.Configuration.IgnoreFilename ?? PluginConfiguration.FILENAME;
        if (ignoreFilename.Length == 0 || parent == null)
        {
            return false;
        }

        BaseItem parentDirectory = parent;
        do
        {
            string ignorePath = Path.Combine(parentDirectory.Path, ignoreFilename);

            if (File.Exists(ignorePath))
            {
                Ignore.Ignore ignore = new Ignore.Ignore();
                File.ReadLines(ignorePath).Where(line => line.Trim().Length > 0).ToList().ForEach(line => ignore.Add(line));

                string relativePath = Path.GetRelativePath(parentDirectory.Path, fileInfo.FullName);

                if (ignore.IsIgnored(relativePath))
                {
                    _logger.LogInformation("Media File {FileName} has been ignored by {IgnoreFile}", fileInfo.FullName, ignorePath);

                    return true;
                }
            }

            parentDirectory = parentDirectory.GetParent();
        }
        while (!parentDirectory.IsTopParent);

        return false;
    }
}
