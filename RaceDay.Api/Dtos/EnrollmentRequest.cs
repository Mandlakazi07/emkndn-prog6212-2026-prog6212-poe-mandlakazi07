// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// What a Participant sends to enter an event: which event, and which category within it.
// The participant is NOT sent: the API takes it from the login cookie.
public class EnrollmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid EventID is required.")]
    public int EventID { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid CategoryID is required.")]
    public int CategoryID { get; set; }
}
