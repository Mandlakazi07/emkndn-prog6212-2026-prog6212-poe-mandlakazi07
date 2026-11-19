// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 17 November 2026].

namespace RaceDay.Web.Models.Api;

// A race result as the API describes it
public class ResultDto
{
    public int ResultID { get; set; }
    public int EnrollmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
    public int TotalFinishers { get; set; }
}
