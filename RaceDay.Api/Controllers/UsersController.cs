// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 18 October 2026].

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    private static UserResponse ToResponse(User user) => new()
    {
        UserID = user.UserID,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        CreatedAt = user.CreatedAt
    };

    /// <summary>Returns the logged-in user's own profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = HttpContext.GetCurrentUserId()!.Value;
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == userId);

        return user is null ? NotFound(new { message = "User not found." }) : Ok(ToResponse(user));
    }

    /// <summary>Updates the logged-in user's own name and email. The role cannot be changed.</summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request)
    {
        var userId = HttpContext.GetCurrentUserId()!.Value;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email && u.UserID != userId))
        {
            return Conflict(new { message = "Another account already uses this email." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Another account already uses this email." });
        }

        return Ok(ToResponse(user));
    }
}
