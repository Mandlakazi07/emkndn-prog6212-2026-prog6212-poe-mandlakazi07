// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Upload files in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads [Accessed 17 November 2026].
//              Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// The Participant's own pages: their enrollments, their results and their profile
[Authorize(Roles = "Participant")]
[Route("Participant")]
public class ParticipantController : AppController
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api)
    {
        _api = api;
    }

    // ----- My events -----

    [HttpGet("MyEvents")]
    public async Task<IActionResult> MyEvents()
    {
        var result = await _api.GetMyEnrollmentsAsync();
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        return View(new MyEventsViewModel
        {
            Enrollments = (result.Data ?? new()).OrderByDescending(e => e.EventDate).ToList(),
            ErrorMessage = result.Success ? null : result.Error
        });
    }

    [HttpPost("MyEvents/{id:int}/Withdraw")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        var result = await _api.WithdrawEnrollmentAsync(id);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        if (result.Success)
        {
            TempData["Success"] = "Your enrollment was withdrawn.";
        }
        else
        {
            // For example: "A result has been recorded for this enrollment, so it cannot be withdrawn."
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(MyEvents));
    }

    // ----- My results -----

    [HttpGet("MyResults")]
    public async Task<IActionResult> MyResults()
    {
        var result = await _api.GetMyResultsAsync();
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        return View(new MyResultsViewModel
        {
            Results = result.Data ?? new(),
            ErrorMessage = result.Success ? null : result.Error
        });
    }

    // ----- Profile -----

    [HttpGet("Profile")]
    public async Task<IActionResult> Profile()
    {
        var result = await _api.GetMyProfileAsync();
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }
        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Error ?? "Your profile could not be loaded.";
            return RedirectToAction("Index", "Home");
        }

        return View(new ProfileViewModel
        {
            User = result.Data,
            Form = new ProfileFormViewModel { FullName = result.Data.FullName, Email = result.Data.Email }
        });
    }

    [HttpPost("Profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfile([Bind(Prefix = "Form")] ProfileFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return await ReloadProfileViewAsync(form);
        }

        var result = await _api.UpdateMyProfileAsync(form.FullName, form.Email);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Your profile could not be updated.");
            return await ReloadProfileViewAsync(form);
        }

        // The name and email shown in the menu come from the login cookie, so refresh the cookie too.
        // Every other claim (ID, role, API cookie) is kept exactly as it was.
        var claims = User.Claims
            .Where(c => c.Type != ClaimTypes.Name && c.Type != ClaimTypes.Email)
            .Concat(new[]
            {
                new Claim(ClaimTypes.Name, result.Data.FullName),
                new Claim(ClaimTypes.Email, result.Data.Email)
            });
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        TempData["Success"] = "Your profile was updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost("Profile/Picture")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        if (picture is null || picture.Length == 0)
        {
            TempData["Error"] = "Please choose a picture first.";
            return RedirectToAction(nameof(Profile));
        }

        var result = await _api.UploadProfilePictureAsync(picture);
        if (await HandleExpiredLoginAsync(result) is IActionResult expired)
        {
            return expired;
        }

        if (result.Success)
        {
            TempData["Success"] = "Your profile picture was updated.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Profile));
    }

    // When a form has errors, show the page again with the details the person typed
    private async Task<IActionResult> ReloadProfileViewAsync(ProfileFormViewModel form)
    {
        var current = await _api.GetMyProfileAsync();
        return View("Profile", new ProfileViewModel
        {
            User = current.Data ?? new(),
            Form = form
        });
    }
}
