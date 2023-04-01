using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Collections;
using System.Text.Json;

namespace Movies.Client.Services;

public class RemoteStreamingSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public RemoteStreamingSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync()
    {
        await GetStreamingMoviesAsync();
    }

    private async Task GetStreamingMoviesAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/moviesstream");

        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        // Regular deserialization
        //var content = await response.Content.ReadAsStringAsync();
        //var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content, _jsonSerializerOptionsWrapper.Options);
        //foreach (var movie in movies)
        //{
        //    Console.WriteLine(movie?.Title);
        //}

        // Deserialization as steam comes in
        // To make this more visable in the console, set Options.DefaultBufferSize in _jsonSerializerOptionsWrapper
        var responseStream = await response.Content.ReadAsStreamAsync();
        var movies = JsonSerializer.DeserializeAsyncEnumerable<Movie>(responseStream, _jsonSerializerOptionsWrapper.Options);

        await foreach (var movie in movies)
        {
            Console.WriteLine(movie?.Title);
        }

    }



}
