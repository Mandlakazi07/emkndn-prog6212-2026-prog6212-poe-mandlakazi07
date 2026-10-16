// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 16 October 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// What a new user sends to register. The validation attributes make the API reject bad input with a 400.
public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    // BCrypt only uses the first 72 bytes of a password, so we cap it there
    [Required, MinLength(8), MaxLength(72)]
    public string Password { get; set; } = string.Empty;

    [Required, RegularExpression("^(Organizer|Participant)$", ErrorMessage = "Role must be Organizer or Participant.")]
    public string Role { get; set; } = string.Empty;
}
