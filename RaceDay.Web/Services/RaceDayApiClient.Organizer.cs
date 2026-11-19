// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Upload files in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads [Accessed 17 November 2026].

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Services;

// The Organizer's calls to the API. This is the same class as RaceDayApiClient (it is "partial"), kept in its own file.
public partial class RaceDayApiClient
{
    // ----- Venues -----

    public Task<ApiResult<List<VenueDto>>> GetVenuesAsync() =>
        GetAsync<List<VenueDto>>("api/venues");

    public Task<ApiResult<VenueDto>> CreateVenueAsync(string name, string address, string city) =>
        SendJsonAsync<VenueDto>(HttpMethod.Post, "api/venues", new { name, address, city });

    // ----- Events -----

    // The logged-in Organizer's own events, past ones included
    public Task<ApiResult<List<EventDto>>> GetMyEventsAsync() =>
        GetAsync<List<EventDto>>("api/events?mine=true");

    public Task<ApiResult<EventDto>> GetEventAsync(int id) =>
        GetAsync<EventDto>($"api/events/{id}");

    public Task<ApiResult<EventDto>> CreateEventAsync(string name, string? description, DateOnly eventDate,
        decimal distance, string eventType, int venueId) =>
        SendJsonAsync<EventDto>(HttpMethod.Post, "api/events",
            new { name, description, eventDate, distance, eventType, venueID = venueId });

    public Task<ApiResult<EventDto>> UpdateEventAsync(int id, string name, string? description, DateOnly eventDate,
        decimal distance, string eventType, int venueId) =>
        SendJsonAsync<EventDto>(HttpMethod.Put, $"api/events/{id}",
            new { name, description, eventDate, distance, eventType, venueID = venueId });

    public Task<ApiResult<NoData>> DeleteEventAsync(int id) =>
        SendJsonAsync<NoData>(HttpMethod.Delete, $"api/events/{id}", null);

    // The picture is sent to the API, which saves it to Azure Blob Storage. The web app never talks to Azure.
    public Task<ApiResult<ImageUploadDto>> UploadEventBannerAsync(int eventId, IFormFile file) =>
        UploadImageAsync($"api/events/{eventId}/banner", file);

    // ----- Categories -----

    public Task<ApiResult<List<CategoryDto>>> GetCategoriesAsync(int eventId) =>
        GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");

    public Task<ApiResult<CategoryDto>> CreateCategoryAsync(int eventId, string categoryName, int? minAge, int? maxAge, decimal? distanceKm) =>
        SendJsonAsync<CategoryDto>(HttpMethod.Post, $"api/events/{eventId}/categories",
            new { categoryName, minAge, maxAge, distanceKm });

    public Task<ApiResult<CategoryDto>> UpdateCategoryAsync(int id, string categoryName, int? minAge, int? maxAge, decimal? distanceKm) =>
        SendJsonAsync<CategoryDto>(HttpMethod.Put, $"api/categories/{id}",
            new { categoryName, minAge, maxAge, distanceKm });

    public Task<ApiResult<NoData>> DeleteCategoryAsync(int id) =>
        SendJsonAsync<NoData>(HttpMethod.Delete, $"api/categories/{id}", null);

    // ----- Enrollments and results (Organizer side) -----

    public Task<ApiResult<List<EnrollmentDto>>> GetEventEnrollmentsAsync(int eventId) =>
        GetAsync<List<EnrollmentDto>>($"api/events/{eventId}/enrollments");

    public Task<ApiResult<EnrollmentDto>> UpdateEnrollmentStatusAsync(int enrollmentId, string status) =>
        SendJsonAsync<EnrollmentDto>(HttpMethod.Put, $"api/enrollments/{enrollmentId}/status", new { status });

    public Task<ApiResult<List<ResultDto>>> GetEventResultsAsync(int eventId) =>
        GetAsync<List<ResultDto>>($"api/events/{eventId}/results");

    // finishTime is written as hh:mm:ss, for example "01:45:32"
    public Task<ApiResult<ResultDto>> RecordResultAsync(int enrollmentId, string finishTime, int finishPosition) =>
        SendJsonAsync<ResultDto>(HttpMethod.Post, "api/results",
            new { enrollmentID = enrollmentId, finishTime, finishPosition });

    // ----- Image upload (used by the banner and, later, the profile picture) -----

    private async Task<ApiResult<ImageUploadDto>> UploadImageAsync(string url, IFormFile file)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // The API ignores the original file name and chooses its own random name
            content.Add(fileContent, "file", "upload");

            using var response = await _http.PostAsync(url, content);
            return await ToResultAsync<ImageUploadDto>(response);
        }
        catch (FormatException)
        {
            return ApiResult<ImageUploadDto>.Fail(HttpStatusCode.BadRequest, "That file type is not supported.");
        }
        catch (HttpRequestException)
        {
            return ApiResult<ImageUploadDto>.Unavailable();
        }
        catch (TaskCanceledException)
        {
            return ApiResult<ImageUploadDto>.Unavailable();
        }
        catch (JsonException)
        {
            return ApiResult<ImageUploadDto>.Fail(HttpStatusCode.BadGateway, UnreadableResponse);
        }
    }
}
