using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Collections;
using System.Text.Json;

namespace Movies.Client.Services;

public class HttpClientFactorySamples : IIntegrationService
{
    private readonly MoviesAPIClient _moviesAPIClient;

    public async Task RunAsync()
    {
        await GetMoviesViaMoviesAPIClientAsync();
    }

    public HttpClientFactorySamples(MoviesAPIClient moviesAPIClient)
    {
        _moviesAPIClient = moviesAPIClient ?? throw new ArgumentNullException(nameof(moviesAPIClient));
    }

    private async Task GetMoviesViaMoviesAPIClientAsync()
    {
        var movies = await _moviesAPIClient.GetMoviesAsync();
    }

}
