using Microsoft.AspNetCore.Mvc;
using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Text.Json;

namespace Movies.Client.Services;

public class FaultsAndErrorsSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public FaultsAndErrorsSamples(JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper, IHttpClientFactory httpClientFactory)
    {
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper;
        _httpClientFactory = httpClientFactory;
    }

    public async Task RunAsync()
    {
        await GetMovieAndDealWithInvalidResponsesAsync(CancellationToken.None);
        await PostMovieAndHandleErrorsAsync(CancellationToken.None);
    }

    private async Task GetMovieAndDealWithInvalidResponsesAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var request = new HttpRequestMessage(HttpMethod.Get, "api/movies/030a43b0-f9a5-405a-811c-bf342524b2be");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));

        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)) 
        {
            if (!response.IsSuccessStatusCode)
            {
                // Inspect statuscode
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("The requested movice cannot be found");
                    return;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return;
                }
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var movie = await JsonSerializer.DeserializeAsync<Movie>(stream, _jsonSerializerOptionsWrapper.Options);
        }

    }

    private async Task PostMovieAndHandleErrorsAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var movieForCreation = new MovieForCreation()
        {
            Title = "Pulp",
            Description = "That movie",
            DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
            ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
            Genre = "Crime, Drama"
        };

        var serializedMovieForCreation = JsonSerializer.Serialize(movieForCreation, _jsonSerializerOptionsWrapper.Options);

        using (var request = new HttpRequestMessage(HttpMethod.Post, "api/movies"))
        {
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            request.Content = new StringContent(serializedMovieForCreation);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    // Inspect statuscode
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var errorStream = await response.Content.ReadAsStreamAsync();

                        var errorAsProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(errorStream, _jsonSerializerOptionsWrapper.Options);

                        var errors = errorAsProblemDetails?.Errors;
                        Console.WriteLine(errorAsProblemDetails?.Title);

                        return;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return;   
                    }
                    response.EnsureSuccessStatusCode();
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var movie = await JsonSerializer.DeserializeAsync<Movie>(stream, _jsonSerializerOptionsWrapper.Options);
            }
        
        }
    }
} 