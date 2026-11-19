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

// Categories for one event: add, edit and delete (age range and distance are optional)
[Authorize(Roles = "Organizer")]
[Route("Organizer/Events/{eventId:int}/Categories")]
public class OrganizerCategoriesController : AppController
{
    private readonly RaceDayApiClient _api;

    public OrganizerCategoriesController(RaceDayApiClient api)
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

        var categories = await _api.GetCategoriesAsync(eventId);
        return View(new EventCategoriesViewModel
        {
            Event = eventResult.Data,
            Categories = categories.Data ?? new(),
            NewCategory = new CategoryFormViewModel { EventID = eventId }
        });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int eventId, [Bind(Prefix = "NewCategory")] CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = FirstError();
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var result = await _api.CreateCategoryAsync(eventId, model.CategoryName, model.MinAge, model.MaxAge, model.DistanceKm);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        if (result.Success)
        {
            TempData["Success"] = $"Category \"{model.CategoryName}\" added.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int eventId, int id)
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

        var category = eventResult.Data.Categories.FirstOrDefault(c => c.CategoryID == id);
        if (category is null)
        {
            TempData["Error"] = "That category could not be found.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        ViewData["EventName"] = eventResult.Data.Name;
        return View(new CategoryFormViewModel
        {
            CategoryID = category.CategoryID,
            EventID = eventId,
            CategoryName = category.CategoryName,
            MinAge = category.MinAge,
            MaxAge = category.MaxAge,
            DistanceKm = category.DistanceKm
        });
    }

    [HttpPost("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int eventId, int id, CategoryFormViewModel model)
    {
        model.EventID = eventId;
        model.CategoryID = id;
        ViewData["EventName"] = "this event";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _api.UpdateCategoryAsync(id, model.CategoryName, model.MinAge, model.MaxAge, model.DistanceKm);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The category could not be updated.");
            return View(model);
        }

        TempData["Success"] = $"Category \"{model.CategoryName}\" updated.";
        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpPost("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int eventId, int id)
    {
        var result = await _api.DeleteCategoryAsync(id);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        if (result.Success)
        {
            TempData["Success"] = "The category was deleted.";
        }
        else
        {
            // For example: "Participants have enrolled in this category, so it cannot be deleted."
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }

    private string FirstError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault()
        ?? "Please check the details and try again.";
}
