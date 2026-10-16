// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, 2023. Introduction to relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Venue
{
    [Key]
    public int VenueID { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
