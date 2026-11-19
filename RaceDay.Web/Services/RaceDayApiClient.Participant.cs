// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Upload files in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads [Accessed 17 November 2026].

using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Services;

// The Participant's calls to the API (the same partial class as RaceDayApiClient)
public partial class RaceDayApiClient
{
    // ----- Browsing events (public) -----

    // Upcoming events, optionally filtered. The API does the filtering.
    public Task<ApiResult<List<EventDto>>> GetEventsAsync(string? eventType, string? city, DateOnly? fromDate, DateOnly? toDate)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(eventType)) parts.Add("eventType=" + Uri.EscapeDataString(eventType));
        if (!string.IsNullOrWhiteSpace(city)) parts.Add("city=" + Uri.EscapeDataString(city));
        if (fromDate.HasValue) parts.Add("fromDate=" + fromDate.Value.ToString("yyyy-MM-dd"));
        if (toDate.HasValue) parts.Add("toDate=" + toDate.Value.ToString("yyyy-MM-dd"));

        var url = parts.Count == 0 ? "api/events" : "api/events?" + string.Join("&", parts);
        return GetAsync<List<EventDto>>(url);
    }

    // ----- Enrollments -----

    public Task<ApiResult<EnrollmentDto>> EnrollAsync(int eventId, int categoryId) =>
        SendJsonAsync<EnrollmentDto>(HttpMethod.Post, "api/enrollments", new { eventID = eventId, categoryID = categoryId });

    public Task<ApiResult<List<EnrollmentDto>>> GetMyEnrollmentsAsync() =>
        GetAsync<List<EnrollmentDto>>("api/enrollments/me");

    public Task<ApiResult<NoData>> WithdrawEnrollmentAsync(int enrollmentId) =>
        SendJsonAsync<NoData>(HttpMethod.Delete, $"api/enrollments/{enrollmentId}", null);

    // ----- Results -----

    public Task<ApiResult<List<ResultDto>>> GetMyResultsAsync() =>
        GetAsync<List<ResultDto>>("api/results/me");

    // ----- Profile -----

    public Task<ApiResult<UserDto>> GetMyProfileAsync() =>
        GetAsync<UserDto>("api/users/me");

    public Task<ApiResult<UserDto>> UpdateMyProfileAsync(string fullName, string email) =>
        SendJsonAsync<UserDto>(HttpMethod.Put, "api/users/me", new { fullName, email });

    // The picture goes to the API, which saves it to Azure Blob Storage. The web app never talks to Azure.
    public Task<ApiResult<ImageUploadDto>> UploadProfilePictureAsync(IFormFile file) =>
        UploadImageAsync("api/users/me/profile-picture", file);
}
