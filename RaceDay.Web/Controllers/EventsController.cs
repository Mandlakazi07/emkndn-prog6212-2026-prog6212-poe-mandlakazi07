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

// The public event page, and the Participant's "Enter this event" action
[Route("Events")]
public class EventsController : AppController
{
    private readonly RaceDayApiClient _api;

    public EventsController(RaceDayApiClient api)
    {
        _api = api;
    }

    // Anyone can read the details. Only a logged-in Participant sees the enrol form.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _api.GetEventAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Error ?? "That event could not be found.";
            return RedirectToAction("Index", "Home");
        }

        var model = new EventDetailsViewModel
        {
            Event = result.Data,
            IsParticipant = User.IsInRole("Participant"),
            IsOrganizer = User.IsInRole("Organizer")
        };

        if (model.IsParticipant)
        {
            var mine = await _api.GetMyEnrollmentsAsync();
            if (await HandleExpiredLoginAsync(mine) is IActionResult expired)
            {
                return expired;
            }
            model.MyEnrollment = mine.Data?.FirstOrDefault(e => e.EventID == id);
        }

        return View(model);
    }

    [HttpPost("{id:int}/Enrol")]
    [Authorize(Roles = "Participant")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int id, int categoryId)
    {
        if (categoryId <= 0)
        {
            TempData["Error"] = "Please choose a category first.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await _api.EnrollAsync(id, categoryId);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        if (result.Success)
        {
            TempData["Success"] = "You have entered the event. Your enrollment is pending until the organizer confirms it.";
        }
        else
        {
            // For example: "You are already enrolled in this event." or "This event has already taken place."
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
