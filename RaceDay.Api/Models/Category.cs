// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, 2023. Introduction to relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 17 November 2026].

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Category
{
    [Key]
    public int CategoryID { get; set; }

    public int EventID { get; set; }

    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    // Optional: who and what this category is for, for example ages 20 to 39 over 10 km
    public int? MinAge { get; set; }

    public int? MaxAge { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal? DistanceKm { get; set; }

    public Event Event { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
