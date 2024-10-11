using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private static readonly Dictionary<string, IgnoreCache> _ignoreCaches = new();
    private static readonly object _ignoreCacheLock = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="JFIgnoreFileRule"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public JFIgnoreFileRule(ILogger<JFIgnoreFileRule> logger)
    {
        _logger = logger;
    }

    private Ignore.Ignore? LoadIgnore(string ignorePath)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if (!_ignoreCaches.TryGetValue(ignorePath, out var value) ||
            File.GetLastWriteTime(ignorePath) > value.LastModified)
        {
            lock (_ignoreCacheLock)
            {
                Ignore.Ignore ignore = new Ignore.Ignore();
                File.ReadLines(ignorePath).Where(line => line.Trim().Length > 0).ToList().ForEach(line => ignore.Add(line));

                _ignoreCaches.Remove(ignorePath);
                _ignoreCaches.Add(ignorePath, new IgnoreCache(File.GetLastWriteTime(ignorePath), ignore));
            }
        }

        return value?.Ignore;
    }

    /// <inheritdoc />
    public bool ShouldIgnore(FileSystemMetadata fileInfo, BaseItem? parent)
    {
        string ignoreFilename = Plugin.Instance?.Configuration.IgnoreFilename ?? PluginConfiguration.FILENAME;
        if (ignoreFilename.Length == 0 || parent == null)
        {
            return false;
        }

        _logger.LogDebug("Ignore Filename: {IgnoreFilename}", ignoreFilename);

        BaseItem parentDirectory = parent;
        do
        {
            string ignorePath = Path.Combine(parentDirectory.Path, ignoreFilename);

            _logger.LogDebug("Check Ignore Path: {IgnorePath}", ignorePath);

            if (File.Exists(ignorePath))
            {
                try
                {
                    Ignore.Ignore? ignore = LoadIgnore(ignorePath);
                    if (ignore == null)
                    {
                        continue;
                    }

                    string relativePath = Path.GetRelativePath(parentDirectory.Path, fileInfo.FullName);

                    if (ignore.IsIgnored(relativePath))
                    {
                        _logger.LogInformation("Media File {FileName} has been ignored by {IgnoreFile}", fileInfo.FullName, ignorePath);

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Error handling Ignore File {IgnoreFile} on {FileName}", ignorePath, fileInfo.FullName);
                }
            }

            parentDirectory = parentDirectory.GetParent();

            _logger.LogDebug("Parent Directory: {ParentDirectory}", parentDirectory);
        }
        while (parentDirectory is { IsTopParent: false });

        return false;
    }

    private sealed class IgnoreCache(DateTime lastModified, Ignore.Ignore ignore)
    {
        public DateTime LastModified { get; init; } = lastModified;

        public Ignore.Ignore Ignore { get; init; } = ignore;
    }
}
