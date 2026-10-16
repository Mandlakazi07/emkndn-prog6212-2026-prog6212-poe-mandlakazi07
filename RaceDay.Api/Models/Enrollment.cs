// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Enrollment
{
    [Key]
    public int EnrollmentID { get; set; }

    // Foreign keys
    public int ParticipantID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }

    public DateTime EnrollmentDate { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    // Navigation properties
    public User Participant { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Result? Result { get; set; }
}
