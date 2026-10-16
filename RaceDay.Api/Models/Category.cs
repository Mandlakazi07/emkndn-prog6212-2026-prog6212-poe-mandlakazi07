// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, 2023. Introduction to relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Category
{
    [Key]
    public int CategoryID { get; set; }

    public int EventID { get; set; }

    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    public Event Event { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
