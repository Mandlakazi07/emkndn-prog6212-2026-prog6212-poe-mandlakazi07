// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 3 November 2026].

namespace RaceDay.Web.Models.Api;

// A user as the API describes them. The web app never sees a password or a hash.
public class UserDto
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
