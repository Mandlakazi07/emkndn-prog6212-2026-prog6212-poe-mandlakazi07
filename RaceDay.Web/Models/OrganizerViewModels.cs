// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Models;

// One event on the dashboard, with how many people have entered it
public class EventSummary
{
    public EventDto Event { get; set; } = new();
    public int Enrolled { get; set; }    // everyone except cancelled enrollments
    public int Confirmed { get; set; }
    public int Pending { get; set; }
}

public class OrganizerDashboardViewModel
{
    public List<EventSummary> Events { get; set; } = new();
    public string? ErrorMessage { get; set; }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public int TotalEvents => Events.Count;
    public int UpcomingCount => Events.Count(e => e.Event.EventDate >= Today);
    public int TotalEnrolled => Events.Sum(e => e.Enrolled);
    public int TotalPending => Events.Sum(e => e.Pending);

    public List<EventSummary> Upcoming =>
        Events.Where(e => e.Event.EventDate >= Today).OrderBy(e => e.Event.EventDate).Take(5).ToList();
}

public class EventsIndexViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class EventCategoriesViewModel
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public CategoryFormViewModel NewCategory { get; set; } = new();
}

public class EventEnrollmentsViewModel
{
    public EventDto Event { get; set; } = new();
    public List<EnrollmentDto> Enrollments { get; set; } = new();
}

// One line on the results page: an enrollment, and its result if one has been recorded
public class ResultRow
{
    public EnrollmentDto Enrollment { get; set; } = new();
    public ResultDto? Result { get; set; }
}

public class EventResultsViewModel
{
    public EventDto Event { get; set; } = new();
    public List<ResultRow> Rows { get; set; } = new();
    public bool EventHasStarted { get; set; }
}

public class VenuesIndexViewModel
{
    public List<VenueDto> Venues { get; set; } = new();
    public VenueFormViewModel NewVenue { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
