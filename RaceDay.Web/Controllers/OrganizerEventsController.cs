// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Upload files in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads [Accessed 17 November 2026].

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Organizers manage their events here: list, create, edit, delete, and upload a banner picture
[Authorize(Roles = "Organizer")]
[Route("Organizer/Events")]
public class OrganizerEventsController : AppController
{
    private readonly RaceDayApiClient _api;

    public OrganizerEventsController(RaceDayApiClient api)
    {
        _api = api;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetMyEventsAsync();
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        return View(new EventsIndexViewModel
        {
            Events = (result.Data ?? new()).OrderByDescending(e => e.EventDate).ToList(),
            ErrorMessage = result.Success ? null : result.Error
        });
    }

    // ----- Create -----

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new EventFormViewModel();
        await LoadVenuesAsync(model);
        return View("Form", model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        await LoadVenuesAsync(model);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var created = await _api.CreateEventAsync(model.Name, model.Description, model.EventDate!.Value,
            model.Distance!.Value, model.EventType, model.VenueID);
        if (await HandleExpiredLoginAsync(created) is IActionResult expired)
        {
            return expired;
        }
        if (!created.Success)
        {
            ModelState.AddModelError(string.Empty, created.Error ?? "The event could not be created.");
            return View("Form", model);
        }

        TempData["Success"] = $"Event \"{model.Name}\" created.";
        await UploadBannerIfChosenAsync(created.Data!.EventID, model.BannerFile);
        return RedirectToAction(nameof(Index));
    }

    // ----- Edit -----

    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _api.GetEventAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Error ?? "That event could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var ev = result.Data;
        if (ev.OrganizerID != CurrentUserId)
        {
            return Forbid(); // Organizers can only edit their own events
        }

        var model = new EventFormViewModel
        {
            EventID = ev.EventID,
            Name = ev.Name,
            Description = ev.Description,
            EventDate = ev.EventDate,
            Distance = ev.Distance,
            EventType = ev.EventType,
            VenueID = ev.Venue.VenueID,
            BannerImageUrl = ev.BannerImageUrl
        };
        await LoadVenuesAsync(model);
        return View("Form", model);
    }

    [HttpPost("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        model.EventID = id;
        await LoadVenuesAsync(model);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var updated = await _api.UpdateEventAsync(id, model.Name, model.Description, model.EventDate!.Value,
            model.Distance!.Value, model.EventType, model.VenueID);
        if (await HandleExpiredLoginAsync(updated) is IActionResult expired)
        {
            return expired;
        }
        if (!updated.Success)
        {
            // For example 403 if the event belongs to someone else
            ModelState.AddModelError(string.Empty, updated.Error ?? "The event could not be updated.");
            return View("Form", model);
        }

        TempData["Success"] = $"Event \"{model.Name}\" updated.";
        await UploadBannerIfChosenAsync(id, model.BannerFile);
        return RedirectToAction(nameof(Index));
    }

    // ----- Delete -----

    [HttpPost("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteEventAsync(id);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        if (result.Success)
        {
            TempData["Success"] = "The event was deleted.";
        }
        else
        {
            // For example: "This event has enrollments and cannot be deleted."
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }

    // ----- Helpers -----

    private async Task LoadVenuesAsync(EventFormViewModel model)
    {
        var venues = await _api.GetVenuesAsync();
        model.Venues = venues.Data ?? new();
        if (!venues.Success)
        {
            ModelState.AddModelError(string.Empty, venues.Error ?? "The venues could not be loaded.");
        }
    }

    // If a picture was chosen, send it to the API. A problem here never undoes the saved event.
    private async Task UploadBannerIfChosenAsync(int eventId, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return;
        }

        var upload = await _api.UploadEventBannerAsync(eventId, file);
        if (upload.Success)
        {
            TempData["Success"] = (TempData["Success"] as string) + " The banner picture was uploaded.";
        }
        else
        {
            TempData["Error"] = "The event was saved, but the banner could not be uploaded: " + upload.Error;
        }
    }
}
