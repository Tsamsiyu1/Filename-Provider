using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Template;
using Jellyfin.Plugin.Template.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Template;

/// <summary>
/// moviprovider claass.
/// </summary>
public class MovieProvider : IRemoteMetadataProvider<Movie, MovieInfo>
{
    private readonly ILogger _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovieProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public MovieProvider(ILogger<MovieProvider> logger)
    {
        _log = logger;
    }

    /// <summary>
    /// Gets the name of the provider.
    /// </summary>
    public string Name => "Filename Provider (Movies)";

/// <summary>
/// function to start metadata creation.
/// </summary>
/// <param name="searchInfo">the name of the file.</param>
/// <param name="cancellationToken">i don't know.</param>
/// <returns>Movie MEtadata.</returns>
    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        MovieInfo searchInfo,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<RemoteSearchResult>>(Array.Empty<RemoteSearchResult>());
    }

/// <summary>
/// Gets metadata for a given movie. In this example, we check if the name contains "[local]", and if so, we return metadata with the name cleaned up. This is just an example of how you could use the plugin configuration to alter behavior. You can replace this with your own logic to fetch metadata from an external source or use other information from the MovieInfo object.
/// </summary>
/// <param name="info">the movie info.</param>
/// <param name="cancellationToken">i don't know.</param>
/// <returns>movie.</returns>
    public Task<MetadataResult<Movie>> GetMetadata(
        MovieInfo info,
        CancellationToken cancellationToken)
    {
        if (info?.Name == null)
        {
            return Task.FromResult<MetadataResult<Movie>>(null!);
        }

        var cfg = Plugin.Instance?.Configuration ?? new PluginConfiguration();

        if (cfg.LocalTag && !info.Name.Contains("[local]", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<MetadataResult<Movie>>(null!);
        }

        string oldName = info.Name;

        var clean = Regex.Replace(info.Name, @"\s*\[local\]\s*", string.Empty, RegexOptions.IgnoreCase);

        _log.LogInformation("Creating metadata for {0}. New name is {1}", oldName, clean);

        var movie = new Movie
        {
            Name = clean,
            OriginalTitle = clean,
            Overview = "This Movie was added with the Fileame Plugin."
        };

        return Task.FromResult(new MetadataResult<Movie>
        {
            Item = movie,
            HasMetadata = true,
            Provider = Name
        });
    }

/*var movie = new Movie
{
    Name = clean,
    OriginalTitle = clean,
    Overview = "Brief plot here",
    Genres = new List<string> { "Action", "Adventure" },
    ProductionYear = 2020,
    CommunityRating = 7.5,
    OfficialRating = "PG-13",
    Studios = new List<string> { "Studio Name" },
    ProviderIds = new System.Collections.Generic.Dictionary<string,string>
    {
        ["imdb"] = "tt1234567"
    },
    People = new List<Person>
    {
        new Person { Name = "Actor One", Type = PersonType.Actor, Role = "Lead" },
        new Person { Name = "Director One", Type = PersonType.Director }
    }
};*/

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
                // var httpClient = Plugin.Instance!.GetHttpClient();
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
