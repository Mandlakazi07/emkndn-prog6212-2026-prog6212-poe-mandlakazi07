// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 3 November 2026].
//              Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 3 November 2026].

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Services;

// The ONLY class in the web app that talks to the API. The web app has no database code and never calls Azure:
// everything goes through the API over HTTP. It is "partial" so each later pack can add its own methods
// in a separate file (Organizer methods, Participant methods) without editing this one.
public partial class RaceDayApiClient
{
    // The name the API gives its login cookie
    public const string ApiCookieName = "RaceDay.Auth";

    private const string UnreadableResponse = "The website could not read the answer from the RaceDay service.";

    private readonly HttpClient _http;

    public RaceDayApiClient(HttpClient http)
    {
        _http = http;
    }

    // ----- Account -----

    public Task<ApiResult<UserDto>> RegisterAsync(string fullName, string email, string password, string role) =>
        SendJsonAsync<UserDto>(HttpMethod.Post, "api/auth/register", new { fullName, email, password, role });

    // Logs in through the API. On success we also keep the API's login cookie value, so we can send it on later calls.
    public async Task<ApiResult<LoginResult>> LoginAsync(string email, string password)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<LoginResult>.Fail(response.StatusCode, await ReadErrorAsync(response));
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            var cookie = ExtractCookieValue(response, ApiCookieName);

            if (user is null || cookie is null)
            {
                return ApiResult<LoginResult>.Fail(HttpStatusCode.BadGateway,
                    "Login worked, but the website could not read the login details. Please try again.");
            }

            return ApiResult<LoginResult>.Ok(new LoginResult(user, cookie), response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiResult<LoginResult>.Unavailable();
        }
        catch (TaskCanceledException)
        {
            return ApiResult<LoginResult>.Unavailable();
        }
        catch (JsonException)
        {
            return ApiResult<LoginResult>.Fail(HttpStatusCode.BadGateway, UnreadableResponse);
        }
    }

    public Task<ApiResult<NoData>> LogoutAsync() =>
        SendJsonAsync<NoData>(HttpMethod.Post, "api/auth/logout", null);

    // ----- Events (public) -----

    public Task<ApiResult<List<EventDto>>> GetUpcomingEventsAsync() =>
        GetAsync<List<EventDto>>("api/events");

    // ----- Helpers shared by every method (here and in the other partial files) -----

    private async Task<ApiResult<T>> GetAsync<T>(string url)
    {
        try
        {
            using var response = await _http.GetAsync(url);
            return await ToResultAsync<T>(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Unavailable();
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Unavailable();
        }
        catch (JsonException)
        {
            return ApiResult<T>.Fail(HttpStatusCode.BadGateway, UnreadableResponse);
        }
    }

    private async Task<ApiResult<T>> SendJsonAsync<T>(HttpMethod method, string url, object? body)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            using var response = await _http.SendAsync(request);
            return await ToResultAsync<T>(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Unavailable();
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Unavailable();
        }
        catch (JsonException)
        {
            return ApiResult<T>.Fail(HttpStatusCode.BadGateway, UnreadableResponse);
        }
    }

    private static async Task<ApiResult<T>> ToResultAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<T>.Fail(response.StatusCode, await ReadErrorAsync(response));
        }

        // 204 No Content (for example after a delete) has no body to read
        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
        {
            return ApiResult<T>.Ok(default, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<T>();
        return ApiResult<T>.Ok(data, response.StatusCode);
    }

    // The API reports problems in two shapes: { "message": "..." } or a list of validation errors.
    // This turns either one into a sentence a person can read.
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                {
                    var text = message.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (var item in field.Value.EnumerateArray())
                        {
                            var line = item.GetString();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                messages.Add(line);
                            }
                        }
                    }

                    if (messages.Count > 0)
                    {
                        return string.Join(" ", messages);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // The body was not JSON: fall through to the generic message below
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "You need to log in to do that.",
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.NotFound => "We could not find what you were looking for.",
            HttpStatusCode.Conflict => "That conflicts with something that already exists.",
            HttpStatusCode.BadRequest => "Some of the details were not accepted. Please check them and try again.",
            _ => "Something went wrong. Please try again."
        };
    }

    // Finds the API's login cookie in the response headers and returns just its value
    private static string? ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
        {
            return null;
        }

        var prefix = cookieName + "=";
        foreach (var header in headers)
        {
            if (!header.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            var end = header.IndexOf(';');
            return end < 0 ? header[prefix.Length..] : header[prefix.Length..end];
        }

        return null;
    }
}
