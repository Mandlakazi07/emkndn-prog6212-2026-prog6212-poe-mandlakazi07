// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, 2023. Introduction to relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResultsController(AppDbContext db)
    {
        _db = db;
    }

    // Turns a query of Results into ResultResponses, adding the readable names and the finisher count
    private IQueryable<ResultResponse> ToResponses(IQueryable<Result> query) =>
        query.Select(r => new ResultResponse
        {
            ResultID = r.ResultID,
            EnrollmentID = r.EnrollmentID,
            ParticipantID = r.Enrollment.ParticipantID,
            ParticipantName = r.Enrollment.Participant.FullName,
            EventID = r.Enrollment.EventID,
            EventName = r.Enrollment.Event.Name,
            EventDate = r.Enrollment.Event.EventDate,
            CategoryName = r.Enrollment.Category.CategoryName,
            FinishTime = r.FinishTime,
            FinishPosition = r.FinishPosition,
            // How many results exist for this same event
            TotalFinishers = _db.Results.Count(x => x.Enrollment.EventID == r.Enrollment.EventID)
        });

    /// <summary>Records a finish time and position for an enrollment, after the event. Only the event's Organizer can do this.</summary>
    [HttpPost("results")]
    [SessionAuthorize("Organizer")]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateResult(ResultRequest request)
    {
        var enrollment = await _db.Enrollments.Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.EnrollmentID == request.EnrollmentID);

        if (enrollment is null)
        {
            return NotFound(new { message = "Enrollment not found." });
        }
        if (enrollment.Event.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only record results for your own events." });
        }
        if (enrollment.Event.EventDate > DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new { message = "Results can only be recorded once the event has taken place." });
        }
        if (enrollment.Status == "Cancelled")
        {
            return BadRequest(new { message = "A cancelled enrollment cannot have a result." });
        }
        if (request.FinishTime <= TimeSpan.Zero)
        {
            return BadRequest(new { message = "FinishTime must be greater than zero." });
        }
        if (await _db.Results.AnyAsync(r => r.EnrollmentID == request.EnrollmentID))
        {
            return Conflict(new { message = "A result has already been recorded for this enrollment." });
        }

        var result = new Result
        {
            EnrollmentID = request.EnrollmentID,
            FinishTime = request.FinishTime,
            FinishPosition = request.FinishPosition
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var created = await ToResponses(_db.Results.AsNoTracking().Where(r => r.ResultID == result.ResultID)).FirstAsync();
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>Returns the logged-in Participant's own race history, newest event first.</summary>
    [HttpGet("results/me")]
    [SessionAuthorize("Participant")]
    [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyResults()
    {
        var participantId = HttpContext.GetCurrentUserId()!.Value;

        var results = await ToResponses(_db.Results.AsNoTracking()
                .Where(r => r.Enrollment.ParticipantID == participantId)
                .OrderByDescending(r => r.Enrollment.Event.EventDate))
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Lists every result recorded for one event, in finishing order. Only that event's Organizer can see this.</summary>
    [HttpGet("events/{eventId:int}/results")]
    [SessionAuthorize("Organizer")]
    [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        var targetEvent = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == eventId);
        if (targetEvent is null)
        {
            return NotFound(new { message = "Event not found." });
        }
        if (targetEvent.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only view results for your own events." });
        }

        var results = await ToResponses(_db.Results.AsNoTracking()
                .Where(r => r.Enrollment.EventID == eventId)
                .OrderBy(r => r.FinishPosition))
            .ToListAsync();

        return Ok(results);
    }
}
