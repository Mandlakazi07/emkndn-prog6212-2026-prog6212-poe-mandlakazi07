// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// The add venue form
public class VenueFormViewModel
{
    [Required(ErrorMessage = "Please enter the venue name.")]
    [StringLength(150, ErrorMessage = "The name can be at most 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the address.")]
    [StringLength(255, ErrorMessage = "The address can be at most 255 characters.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the city.")]
    [StringLength(100, ErrorMessage = "The city can be at most 100 characters.")]
    public string City { get; set; } = string.Empty;
}
