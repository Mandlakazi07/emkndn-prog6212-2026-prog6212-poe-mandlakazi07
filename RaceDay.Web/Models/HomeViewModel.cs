// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 3 November 2026].

using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Models;

// What the home page needs: the upcoming events, or a message if the API could not be reached
public class HomeViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
