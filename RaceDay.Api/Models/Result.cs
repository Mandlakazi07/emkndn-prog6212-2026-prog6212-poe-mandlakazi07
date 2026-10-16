// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Result
{
    [Key]
    public int ResultID { get; set; }

    public int EnrollmentID { get; set; }

    public TimeSpan? FinishTime { get; set; }

    public int? FinishPosition { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
}
