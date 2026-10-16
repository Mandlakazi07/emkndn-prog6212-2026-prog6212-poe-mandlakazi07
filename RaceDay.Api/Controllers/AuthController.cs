// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Session and state management in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/app-state [Accessed 16 October 2026].

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Registers a new user as an Organizer or a Participant.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            // BCrypt adds a random salt and hashes, so the plain password is never stored
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two people registering the same email at the same moment: the UNIQUE index catches the second
            return Conflict(new { message = "An account with this email already exists." });
        }

        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    /// <summary>Logs in with email and password and starts a session holding the UserID and Role.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // One message for both "no such email" and "wrong password", so an attacker cannot tell which was wrong
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Start a clean session, then remember who this is
        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserID);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role);

        return Ok(ToResponse(user));
    }

    /// <summary>Ends the current session.</summary>
    [HttpPost("logout")]
    [SessionAuthorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }

    private static UserResponse ToResponse(User user) => new()
    {
        UserID = user.UserID,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        CreatedAt = user.CreatedAt
    };
}
