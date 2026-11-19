// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 3 November 2026].

using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Services;

// What a successful login gives the web app: who the user is, and the API's login cookie value,
// which the web app must send back to the API on later requests so the API knows who is calling.
public sealed record LoginResult(UserDto User, string ApiCookie);
