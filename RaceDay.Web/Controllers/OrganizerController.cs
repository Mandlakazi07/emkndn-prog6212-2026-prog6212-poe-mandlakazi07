// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 17 November 2026].

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// The Organizer's dashboard. [Authorize(Roles = "Organizer")] means: not logged in -> login page,
// logged in as a Participant -> access denied page.
[Authorize(Roles = "Organizer")]
[Route("Organizer")]
public class OrganizerController : AppController
{
    private readonly RaceDayApiClient _api;

    public OrganizerController(RaceDayApiClient api)
    {
        _api = api;
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var eventsResult = await _api.GetMyEventsAsync();
        if (await HandleExpiredLoginAsync(eventsResult) is IActionResult expired)
        {
            return expired;
        }
        if (!eventsResult.Success)
        {
            return View(new OrganizerDashboardViewModel { ErrorMessage = eventsResult.Error });
        }

        var events = eventsResult.Data ?? new List<EventDto>();

        // Ask the API for each event's enrollments at the same time, instead of one after another
        var lookups = await Task.WhenAll(events.Select(async e => new
        {
            Event = e,
            Enrollments = await _api.GetEventEnrollmentsAsync(e.EventID)
        }));

        var model = new OrganizerDashboardViewModel();
        foreach (var item in lookups)
        {
            var list = item.Enrollments.Data ?? new List<EnrollmentDto>();
            model.Events.Add(new EventSummary
            {
                Event = item.Event,
                Enrolled = list.Count(x => x.Status != "Cancelled"),
                Confirmed = list.Count(x => x.Status == "Confirmed"),
                Pending = list.Count(x => x.Status == "Pending")
            });
        }

        return View(model);
    }
}
