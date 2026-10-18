// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// What an Organizer sends to create or update an event.
// The organizer is NOT sent: the API takes it from the login cookie so nobody can create events as someone else.
public class EventRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly EventDate { get; set; }

    [Range(0.1, 1000)]
    public decimal Distance { get; set; }

    [Required, RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "EventType must be Run, Walk or Cycle.")]
    public string EventType { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "A valid VenueID is required.")]
    public int VenueID { get; set; }
}
