// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        2 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Upload files in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads [Accessed 2 November 2026].
//              Microsoft Learn, n.d. Quickstart: Azure Blob Storage client library for .NET. [online] Available at:
//              https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet [Accessed 2 November 2026].

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

// The two image upload endpoints. The MVC app never talks to Azure: it sends the file here,
// and only this API saves it to Blob Storage and records the URL in the database.
[ApiController]
[Route("api")]
public class ImagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IImageStorage _storage;

    public ImagesController(AppDbContext db, IImageStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    /// <summary>
    /// Uploads a banner image for an event. Only the event's Organizer can do this.
    /// Send a real JPEG, PNG or WebP image of at most 2 MB as multipart/form-data, in a field called "file".
    /// </summary>
    [HttpPost("events/{id:int}/banner")]
    [Authorize(Roles = "Organizer")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5_000_000)]
    [ProducesResponseType(typeof(ImageUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadEventBanner(int id, [FromForm] IFormFile file)
    {
        var existing = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (existing is null)
        {
            return NotFound(new { message = "Event not found." });
        }
        if (existing.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only change the banner of your own events." });
        }

        var image = await ImageValidator.CheckAsync(file);
        if (image is null)
        {
            return BadRequest(new { message = "The file must be a real JPEG, PNG or WebP image of at most 2 MB." });
        }

        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, "events", image.Extension, image.ContentType);

        existing.BannerImageUrl = url;
        await _db.SaveChangesAsync();

        return Ok(new ImageUploadResponse { ImageUrl = url });
    }

    /// <summary>
    /// Uploads a profile picture for the logged-in user (any role).
    /// Send a real JPEG, PNG or WebP image of at most 2 MB as multipart/form-data, in a field called "file".
    /// </summary>
    [HttpPost("users/me/profile-picture")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5_000_000)]
    [ProducesResponseType(typeof(ImageUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile file)
    {
        var userId = HttpContext.GetCurrentUserId()!.Value;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        var image = await ImageValidator.CheckAsync(file);
        if (image is null)
        {
            return BadRequest(new { message = "The file must be a real JPEG, PNG or WebP image of at most 2 MB." });
        }

        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, "profiles", image.Extension, image.ContentType);

        user.ProfilePictureUrl = url;
        await _db.SaveChangesAsync();

        return Ok(new ImageUploadResponse { ImageUrl = url });
    }
}
