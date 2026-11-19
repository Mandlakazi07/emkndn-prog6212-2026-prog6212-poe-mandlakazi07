// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 17 November 2026].

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// The base class for all the web app's controllers
public abstract class AppController : Controller
{
    // The logged-in user's ID, read from the login cookie. Only meaningful on pages that need a login.
    protected int CurrentUserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    // The web app's login cookie and the API's login cookie can run out at slightly different moments.
    // If the API says "you are not logged in" for someone the website thinks IS logged in, their API login has expired.
    // Call this after any API call that needs a login. It returns a redirect to the login page, or null if all is well.
    protected async Task<IActionResult?> HandleExpiredLoginAsync<T>(ApiResult<T> result)
    {
        if (!result.IsUnauthorized || User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Error"] = "Your login has expired. Please log in again.";
        return RedirectToAction("Login", "Account");
    }
}
