// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, 2023. Introduction to relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Event
{
    [Key]
    public int EventID { get; set; }

    // Foreign keys
    public int OrganizerID { get; set; }
    public int VenueID { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly EventDate { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal Distance { get; set; }

    [Required, MaxLength(20)]
    public string EventType { get; set; } = string.Empty;

    // Public URL of the event banner image in Azure Blob Storage
    [MaxLength(500)]
    public string? BannerImageUrl { get; set; }

    // Navigation properties
    public User Organizer { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
