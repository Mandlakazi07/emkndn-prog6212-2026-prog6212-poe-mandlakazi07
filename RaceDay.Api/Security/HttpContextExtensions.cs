// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        17 October 2026
// AI use:     Deepseek helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 17 October 2026].

using System.Security.Claims;

namespace RaceDay.Api.Security;

// Shortcuts for reading who is logged in, from the claims stored in the login cookie
public static class HttpContextExtensions
{
    // The logged-in user's ID, or null when nobody is logged in
    public static int? GetCurrentUserId(this HttpContext context)
    {
        var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var id) ? id : null;
    }

    // The logged-in user's role ("Organizer" or "Participant"), or null
    public static string? GetCurrentRole(this HttpContext context) =>
        context.User.FindFirst(ClaimTypes.Role)?.Value;
}
