// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class HomeController : AppController
{
    private readonly RaceDayApiClient _api;

    public HomeController(RaceDayApiClient api)
    {
        _api = api;
    }

    // The home page lists upcoming events and lets visitors filter them. Anyone can see it, logged in or not.
    public async Task<IActionResult> Index(string? eventType, string? city, DateOnly? fromDate, DateOnly? toDate)
    {
        var model = new HomeViewModel { EventType = eventType, City = city, FromDate = fromDate, ToDate = toDate };

        // All upcoming events: used for the city drop-down, and as the list when no filter is chosen
        var all = await _api.GetUpcomingEventsAsync();
        if (!all.Success)
        {
            model.ErrorMessage = all.Error;
            return View(model);
        }

        var allEvents = all.Data ?? new List<EventDto>();
        model.Cities = allEvents.Select(e => e.Venue.City).Distinct().OrderBy(c => c).ToList();

        if (model.HasFilters)
        {
            // The API does the filtering
            var filtered = await _api.GetEventsAsync(eventType, city, fromDate, toDate);
            if (!filtered.Success)
            {
                model.ErrorMessage = filtered.Error;
                return View(model);
            }
            model.Events = filtered.Data ?? new List<EventDto>();
        }
        else
        {
            model.Events = allEvents;
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
