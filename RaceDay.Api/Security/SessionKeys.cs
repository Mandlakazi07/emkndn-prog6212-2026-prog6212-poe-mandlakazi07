// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Session and state management in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/app-state [Accessed 16 October 2026].

namespace RaceDay.Api.Security;

// The keys under which the logged-in user's details are stored in the session
public static class SessionKeys
{
    public const string UserId = "UserID";
    public const string Role = "Role";
}
