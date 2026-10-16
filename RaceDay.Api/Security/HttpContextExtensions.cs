// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Session and state management in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/app-state [Accessed 16 October 2026].

namespace RaceDay.Api.Security;

// Shortcuts for reading who is logged in from the session
public static class HttpContextExtensions
{
    // The logged-in user's ID, or null when nobody is logged in
    public static int? GetCurrentUserId(this HttpContext context) =>
        context.Session.GetInt32(SessionKeys.UserId);

    // The logged-in user's role ("Organizer" or "Participant"), or null
    public static string? GetCurrentRole(this HttpContext context) =>
        context.Session.GetString(SessionKeys.Role);
}
