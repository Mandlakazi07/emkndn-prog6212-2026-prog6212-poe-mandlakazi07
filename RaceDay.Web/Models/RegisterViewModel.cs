// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 3 November 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// The data on the register form. These rules run in the browser first, then again on the server.
public class RegisterViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, ErrorMessage = "Your name can be at most 100 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "That does not look like an email address.")]
    [StringLength(150, ErrorMessage = "Your email can be at most 150 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a password.")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "Your password must be between 8 and 72 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please repeat your password.")]
    [Compare(nameof(Password), ErrorMessage = "The two passwords do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Repeat password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose your role.")]
    [RegularExpression("^(Organizer|Participant)$", ErrorMessage = "Role must be Organizer or Participant.")]
    public string Role { get; set; } = string.Empty;
}
