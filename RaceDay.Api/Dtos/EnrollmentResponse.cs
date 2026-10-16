// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 16 October 2026].

namespace RaceDay.Api.Dtos;

// What the API returns about an enrollment: readable names as well as IDs,
// so a screen can show "Sipho Dlamini - eThekwini Run - 10km - Pending" without extra requests
public class EnrollmentResponse
{
    public int EnrollmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
