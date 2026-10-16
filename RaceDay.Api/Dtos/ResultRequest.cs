// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// What an Organizer sends to record a participant's result after the event.
// FinishTime is written as hours:minutes:seconds, for example "01:45:32".
public class ResultRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid EnrollmentID is required.")]
    public int EnrollmentID { get; set; }

    public TimeSpan FinishTime { get; set; }

    [Range(1, 100000, ErrorMessage = "FinishPosition must be 1 or more.")]
    public int FinishPosition { get; set; }
}
