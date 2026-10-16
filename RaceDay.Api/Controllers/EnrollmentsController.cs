// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, 2023. Introduction to relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class EnrollmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EnrollmentsController(AppDbContext db)
    {
        _db = db;
    }

    // One reusable recipe for turning an Enrollment into an EnrollmentResponse
    private static readonly Expression<Func<Enrollment, EnrollmentResponse>> ToResponse = x => new EnrollmentResponse
    {
        EnrollmentID = x.EnrollmentID,
        ParticipantID = x.ParticipantID,
        ParticipantName = x.Participant.FullName,
        EventID = x.EventID,
        EventName = x.Event.Name,
        EventDate = x.Event.EventDate,
        CategoryID = x.CategoryID,
        CategoryName = x.Category.CategoryName,
        EnrollmentDate = x.EnrollmentDate,
        Status = x.Status
    };

    /// <summary>Enrols the logged-in Participant in an event under a chosen category. The enrollment starts as Pending.</summary>
    [HttpPost("enrollments")]
    [SessionAuthorize("Participant")]
    [ProducesResponseType(typeof(EnrollmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEnrollment(EnrollmentRequest request)
    {
        var participantId = HttpContext.GetCurrentUserId()!.Value;

        var targetEvent = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == request.EventID);
        if (targetEvent is null)
        {
            return BadRequest(new { message = "That event does not exist." });
        }
        if (targetEvent.EventDate < DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new { message = "This event has already taken place." });
        }
        if (!await _db.Categories.AnyAsync(c => c.CategoryID == request.CategoryID && c.EventID == request.EventID))
        {
            return BadRequest(new { message = "That category does not belong to this event." });
        }
        if (await _db.Enrollments.AnyAsync(x => x.ParticipantID == participantId && x.EventID == request.EventID))
        {
            return Conflict(new { message = "You are already enrolled in this event." });
        }

        var enrollment = new Enrollment
        {
            ParticipantID = participantId,
            EventID = request.EventID,
            CategoryID = request.CategoryID,
            Status = "Pending"
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();

        var created = await _db.Enrollments.AsNoTracking()
            .Where(x => x.EnrollmentID == enrollment.EnrollmentID)
            .Select(ToResponse)
            .FirstAsync();

        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>Lists the logged-in Participant's own enrollments and their status, newest event first.</summary>
    [HttpGet("enrollments/me")]
    [SessionAuthorize("Participant")]
    [ProducesResponseType(typeof(List<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var participantId = HttpContext.GetCurrentUserId()!.Value;

        var enrollments = await _db.Enrollments.AsNoTracking()
            .Where(x => x.ParticipantID == participantId)
            .OrderByDescending(x => x.Event.EventDate)
            .Select(ToResponse)
            .ToListAsync();

        return Ok(enrollments);
    }

    /// <summary>Lists everyone enrolled in one event, with category and status. Only that event's Organizer can see this.</summary>
    [HttpGet("events/{eventId:int}/enrollments")]
    [SessionAuthorize("Organizer")]
    [ProducesResponseType(typeof(List<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventEnrollments(int eventId)
    {
        var targetEvent = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == eventId);
        if (targetEvent is null)
        {
            return NotFound(new { message = "Event not found." });
        }
        if (targetEvent.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only view enrollments for your own events." });
        }

        var enrollments = await _db.Enrollments.AsNoTracking()
            .Where(x => x.EventID == eventId)
            .OrderBy(x => x.Participant.FullName)
            .Select(ToResponse)
            .ToListAsync();

        return Ok(enrollments);
    }

    /// <summary>Changes an enrollment's status (for example Pending to Confirmed). Only the event's Organizer can do this.</summary>
    [HttpPut("enrollments/{id:int}/status")]
    [SessionAuthorize("Organizer")]
    [ProducesResponseType(typeof(EnrollmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEnrollmentStatus(int id, EnrollmentStatusRequest request)
    {
        var enrollment = await _db.Enrollments.Include(x => x.Event).FirstOrDefaultAsync(x => x.EnrollmentID == id);
        if (enrollment is null)
        {
            return NotFound(new { message = "Enrollment not found." });
        }
        if (enrollment.Event.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only change enrollments for your own events." });
        }

        enrollment.Status = request.Status;
        await _db.SaveChangesAsync();

        var updated = await _db.Enrollments.AsNoTracking()
            .Where(x => x.EnrollmentID == id)
            .Select(ToResponse)
            .FirstAsync();

        return Ok(updated);
    }

    /// <summary>Withdraws the logged-in Participant's own enrollment, unless a result has already been recorded for it.</summary>
    [HttpDelete("enrollments/{id:int}")]
    [SessionAuthorize("Participant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(x => x.EnrollmentID == id);
        if (enrollment is null)
        {
            return NotFound(new { message = "Enrollment not found." });
        }
        if (enrollment.ParticipantID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only withdraw your own enrollments." });
        }
        if (await _db.Results.AnyAsync(r => r.EnrollmentID == id))
        {
            return Conflict(new { message = "A result has been recorded for this enrollment, so it cannot be withdrawn." });
        }

        _db.Enrollments.Remove(enrollment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
