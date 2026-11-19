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

public class EventDetailsViewModel
{
    public EventDto Event { get; set; } = new();

    // This Participant's enrollment in this event, if they have one (any status)
    public EnrollmentDto? MyEnrollment { get; set; }

    public bool IsParticipant { get; set; }
    public bool IsOrganizer { get; set; }

    public bool IsPast => Event.EventDate < DateOnly.FromDateTime(DateTime.Today);
    public bool CanEnrol => IsParticipant && MyEnrollment is null && !IsPast && Event.Categories.Any();
}

public class MyEventsViewModel
{
    public List<EnrollmentDto> Enrollments { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class MyResultsViewModel
{
    public List<ResultDto> Results { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class ProfileFormViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, ErrorMessage = "Your name can be at most 100 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "That does not look like an email address.")]
    [StringLength(150, ErrorMessage = "Your email can be at most 150 characters.")]
    public string Email { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    public UserDto User { get; set; } = new();
    public ProfileFormViewModel Form { get; set; } = new();
}
