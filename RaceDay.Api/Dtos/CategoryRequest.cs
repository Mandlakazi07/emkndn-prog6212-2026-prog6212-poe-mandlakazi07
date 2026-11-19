// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// What an Organizer sends to create or update a category.
// The age range and distance are optional, as in the Part 1 plan ("age or distance categories").
public class CategoryRequest : IValidatableObject
{
    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(0, 120)]
    public int? MinAge { get; set; }

    [Range(0, 120)]
    public int? MaxAge { get; set; }

    [Range(0.1, 1000)]
    public decimal? DistanceKm { get; set; }

    // A rule that involves two fields at once, so it cannot be a single attribute
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinAge.HasValue && MaxAge.HasValue && MinAge.Value > MaxAge.Value)
        {
            yield return new ValidationResult(
                "MinAge cannot be greater than MaxAge.",
                new[] { nameof(MinAge), nameof(MaxAge) });
        }
    }
}
