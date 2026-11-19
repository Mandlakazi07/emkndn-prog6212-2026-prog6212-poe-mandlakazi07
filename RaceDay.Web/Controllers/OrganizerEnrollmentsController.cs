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
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Who has entered an event, with their category and status. The Organizer can confirm or cancel an enrollment.
[Authorize(Roles = "Organizer")]
[Route("Organizer/Events/{eventId:int}/Enrollments")]
public class OrganizerEnrollmentsController : AppController
{
    private readonly RaceDayApiClient _api;

    public OrganizerEnrollmentsController(RaceDayApiClient api)
    {
        _api = api;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int eventId)
    {
        var eventResult = await _api.GetEventAsync(eventId);
        if (!eventResult.Success || eventResult.Data is null)
        {
            TempData["Error"] = eventResult.Error ?? "That event could not be found.";
            return RedirectToAction("Index", "OrganizerEvents");
        }
        if (eventResult.Data.OrganizerID != CurrentUserId)
        {
            return Forbid();
        }

        var enrollments = await _api.GetEventEnrollmentsAsync(eventId);
        if (await HandleExpiredLoginAsync(enrollments) is IActionResult expired)
        {
            return expired;
        }
        if (!enrollments.Success)
        {
            TempData["Error"] = enrollments.Error;
        }

        return View(new EventEnrollmentsViewModel
        {
            Event = eventResult.Data,
            Enrollments = enrollments.Data ?? new()
        });
    }

    [HttpPost("{id:int}/Status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int eventId, int id, string status)
    {
        var result = await _api.UpdateEnrollmentStatusAsync(id, status);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        if (result.Success)
        {
            TempData["Success"] = $"{result.Data?.ParticipantName}'s enrollment is now {status}.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }
}
