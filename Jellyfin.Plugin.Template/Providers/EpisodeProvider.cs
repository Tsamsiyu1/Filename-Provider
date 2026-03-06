using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Template;
using Jellyfin.Plugin.Template.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Template;

/// <summary>
/// moviprovider claass.
/// </summary>
public class EpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
{
    private readonly ILogger _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="EpisodeProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public EpisodeProvider(ILogger<EpisodeProvider> logger)
    {
        _log = logger;
    }

    /// <summary>
    /// Gets the name of the provider.
    /// </summary>
    public string Name => "Filename Provider (Episodes)";

/// <summary>
/// function to start metadata creation.
/// </summary>
/// <param name="searchInfo">the episode info used to query the provider.</param>
/// <param name="cancellationToken">cancellation token.</param>
/// <returns>an empty result; this provider does not perform remote searches.</returns>
    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        EpisodeInfo searchInfo,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<RemoteSearchResult>>(Array.Empty<RemoteSearchResult>());
    }

/// <summary>
/// Gets metadata for a given movie. In this example, we check if the name contains "[local]", and if so, we return metadata with the name cleaned up. This is just an example of how you could use the plugin configuration to alter behavior. You can replace this with your own logic to fetch metadata from an external source or use other information from the MovieInfo object.
/// </summary>
/// <param name="info">the movie info.</param>
/// <param name="cancellationToken">i don't know.</param>
/// <returns>episode.</returns>
    public Task<MetadataResult<Episode>> GetMetadata(
        EpisodeInfo info,
        CancellationToken cancellationToken)
    {
        if (info?.Name == null)
        {
            return Task.FromResult<MetadataResult<Episode>>(null!);
        }

        var cfg = Plugin.Instance?.Configuration ?? new PluginConfiguration();

        if (cfg.LocalTag && !info.Name.Contains("[local]", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<MetadataResult<Episode>>(null!);
        }

        string oldName = info.Name;

        var clean = Regex.Replace(info.Name, @"\s*\[local\]\s*", string.Empty, RegexOptions.IgnoreCase);

        _log.LogInformation("Creating metadata for {0}. New name is {1}", oldName, clean);

        var episode = new Episode
        {
            Name = clean,
            OriginalTitle = clean,
            Overview = "This Episode was added with the Fileame Plugin."
        };

        return Task.FromResult(new MetadataResult<Episode>
        {
            Item = episode,
            HasMetadata = true,
            Provider = Name
        });
    }

/// <summary>
/// Get Image.
/// </summary>
/// <param name="url">ab.</param>
/// <param name="cancellationToken">abc.</param>
/// <returns>cba.</returns>
    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        var cfg = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        _log.LogInformation("Image response: {0}", cfg.ImageURL);
        if (!string.IsNullOrWhiteSpace(cfg.ImageURL))
        {
            _log.LogInformation("Attempting to fetch image from URL: {0}", cfg.ImageURL);
            try
            {
                using var client = new HttpClient();
                var resp = await client.GetAsync(cfg.ImageURL, cancellationToken).ConfigureAwait(false);
                _log.LogInformation("Image URL: {0}, Response Status: {1}", cfg.ImageURL, resp.StatusCode);
                return resp;
            }
            catch
            {
                _log.LogInformation("No Image Found!");
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
    }
}
