// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Upload files in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads [Accessed 17 November 2026].

using System.ComponentModel.DataAnnotations;
using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Models;

// The create/edit event form
public class EventFormViewModel
{
    public int EventID { get; set; }

    [Required(ErrorMessage = "Please enter the event name.")]
    [StringLength(150, ErrorMessage = "The name can be at most 150 characters.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Please choose the event date.")]
    [Display(Name = "Event date")]
    public DateOnly? EventDate { get; set; }

    [Required(ErrorMessage = "Please enter the distance in kilometres.")]
    [Range(0.1, 1000, ErrorMessage = "The distance must be between 0.1 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal? Distance { get; set; }

    [Required(ErrorMessage = "Please choose the event type.")]
    [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "Event type must be Run, Walk or Cycle.")]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please choose a venue.")]
    [Display(Name = "Venue")]
    public int VenueID { get; set; }

    // The optional banner picture chosen on the form
    public IFormFile? BannerFile { get; set; }

    // The banner that is already saved (shown when editing)
    public string? BannerImageUrl { get; set; }

    // Not posted by the form: the controller fills these in
    public List<VenueDto> Venues { get; set; } = new();

    public bool IsEdit => EventID > 0;
}
