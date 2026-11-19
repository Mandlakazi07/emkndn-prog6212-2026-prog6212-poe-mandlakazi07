// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 3 November 2026].
//              Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 3 November 2026].

namespace RaceDay.Web.Services;

// A DelegatingHandler works like middleware for outgoing HTTP calls.
// This one adds the logged-in user's API login cookie to every request the web app sends to the API.
// That is how the API knows WHICH user the request is for: the browser never talks to the API, the web app does.
public class ApiCookieHandler : DelegatingHandler
{
    // The name of the claim (inside the web app's own login cookie) that holds the API's login cookie value
    public const string ApiCookieClaim = "api_auth_cookie";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiCookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var apiCookie = _httpContextAccessor.HttpContext?.User.FindFirst(ApiCookieClaim)?.Value;

        if (!string.IsNullOrEmpty(apiCookie))
        {
            request.Headers.TryAddWithoutValidation("Cookie", $"{RaceDayApiClient.ApiCookieName}={apiCookie}");
        }

        return base.SendAsync(request, cancellationToken);
    }
}
