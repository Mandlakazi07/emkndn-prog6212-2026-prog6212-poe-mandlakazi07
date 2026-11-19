// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Models;

// What the home page needs: the (filtered) upcoming events, the filter choices, or a message if the API is unreachable
public class HomeViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public string? ErrorMessage { get; set; }

    // The filters the visitor chose
    public string? EventType { get; set; }
    public string? City { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }

    // The cities that have upcoming events, for the filter drop-down
    public List<string> Cities { get; set; } = new();

    public bool HasFilters =>
        !string.IsNullOrEmpty(EventType) || !string.IsNullOrEmpty(City) || FromDate.HasValue || ToDate.HasValue;
}
