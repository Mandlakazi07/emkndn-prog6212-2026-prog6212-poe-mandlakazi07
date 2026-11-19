// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Results entry: after an event the Organizer records each finisher's time and position.
// A saved result is immediately visible to that Participant on their "My results" page.
[Authorize(Roles = "Organizer")]
[Route("Organizer/Events/{eventId:int}/Results")]
public class OrganizerResultsController : AppController
{
    private readonly RaceDayApiClient _api;

    public OrganizerResultsController(RaceDayApiClient api)
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

        // Ask for the enrollments and the results at the same time
        var enrollmentsTask = _api.GetEventEnrollmentsAsync(eventId);
        var resultsTask = _api.GetEventResultsAsync(eventId);
        await Task.WhenAll(enrollmentsTask, resultsTask);
        var enrollments = enrollmentsTask.Result;
        var results = resultsTask.Result;

        if (await HandleExpiredLoginAsync(enrollments) is IActionResult expired)
        {
            return expired;
        }
        if (!enrollments.Success)
        {
            TempData["Error"] = enrollments.Error;
        }

        var resultList = results.Data ?? new();

        // Cancelled enrollments cannot have a result, so they are left out
        var rows = (enrollments.Data ?? new())
            .Where(e => e.Status != "Cancelled")
            .OrderBy(e => e.ParticipantName)
            .Select(e => new ResultRow
            {
                Enrollment = e,
                Result = resultList.FirstOrDefault(r => r.EnrollmentID == e.EnrollmentID)
            })
            .ToList();

        return View(new EventResultsViewModel
        {
            Event = eventResult.Data,
            Rows = rows,
            EventHasStarted = eventResult.Data.EventDate <= DateOnly.FromDateTime(DateTime.Today)
        });
    }

    [HttpPost("Record")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(int eventId, RecordResultViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault()
                                ?? "Please check the finish time and position.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var result = await _api.RecordResultAsync(model.EnrollmentID, model.FinishTime, model.FinishPosition!.Value);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        if (result.Success)
        {
            TempData["Success"] = $"Result saved for {result.Data?.ParticipantName}.";
        }
        else
        {
            // For example: "Results can only be recorded once the event has taken place."
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }
}
