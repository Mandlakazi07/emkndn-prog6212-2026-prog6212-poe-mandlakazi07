// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, 2023. Introduction to relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class User
{
    [Key]
    public int UserID { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    // "Many" side of the relationships: one user organizes many events, submits many enrollments
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
