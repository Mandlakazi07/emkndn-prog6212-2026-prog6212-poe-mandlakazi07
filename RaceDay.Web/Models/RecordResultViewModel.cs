// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// One row of the results page: the finish time and position for one enrollment
public class RecordResultViewModel
{
    public int EnrollmentID { get; set; }

    [Required(ErrorMessage = "Please enter the finish time.")]
    [RegularExpression(@"^\d{2}:[0-5]\d:[0-5]\d$", ErrorMessage = "Write the finish time as hh:mm:ss, for example 01:45:32.")]
    public string FinishTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the finishing position.")]
    [Range(1, 100000, ErrorMessage = "The position must be 1 or more.")]
    public int? FinishPosition { get; set; }
}
