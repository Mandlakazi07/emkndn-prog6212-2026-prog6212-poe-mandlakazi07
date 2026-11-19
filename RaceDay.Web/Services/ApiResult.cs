// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 3 November 2026].

using System.Net;

namespace RaceDay.Web.Services;

// What comes back from every call to the API: either the data, or a readable error.
// Controllers check Success instead of dealing with exceptions, so a problem shows a friendly message, not a crash.
public class ApiResult<T>
{
    public bool Success { get; private init; }
    public HttpStatusCode StatusCode { get; private init; }
    public T? Data { get; private init; }
    public string? Error { get; private init; }

    // True when the API could not be reached at all (it is stopped, or the network is down)
    public bool ApiUnavailable { get; private init; }

    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
    public bool IsForbidden => StatusCode == HttpStatusCode.Forbidden;
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;

    public static ApiResult<T> Ok(T? data, HttpStatusCode statusCode) =>
        new() { Success = true, StatusCode = statusCode, Data = data };

    public static ApiResult<T> Fail(HttpStatusCode statusCode, string error) =>
        new() { Success = false, StatusCode = statusCode, Error = error };

    public static ApiResult<T> Unavailable() =>
        new()
        {
            Success = false,
            ApiUnavailable = true,
            StatusCode = HttpStatusCode.ServiceUnavailable,
            Error = "The RaceDay service is not available right now. Please try again in a moment."
        };
}

// Used when we only care whether a call worked and not what it returned (for example logout)
public sealed class NoData
{
}
