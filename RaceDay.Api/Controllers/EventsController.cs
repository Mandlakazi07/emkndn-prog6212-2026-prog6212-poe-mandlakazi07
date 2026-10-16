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
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db)
    {
        _db = db;
    }

    // One reusable recipe for turning an Event into an EventResponse (EF turns this into a single SQL query)
    private static readonly Expression<Func<Event, EventResponse>> ToResponse = e => new EventResponse
    {
        EventID = e.EventID,
        Name = e.Name,
        Description = e.Description,
        EventDate = e.EventDate,
        Distance = e.Distance,
        EventType = e.EventType,
        OrganizerID = e.OrganizerID,
        OrganizerName = e.Organizer.FullName,
        Venue = new VenueResponse
        {
            VenueID = e.Venue.VenueID,
            Name = e.Venue.Name,
            Address = e.Venue.Address,
            City = e.Venue.City
        },
        Categories = e.Categories
            .Select(c => new CategoryResponse { CategoryID = c.CategoryID, EventID = c.EventID, CategoryName = c.CategoryName })
            .ToList()
    };

    /// <summary>
    /// Lists upcoming events. Optional filters: eventType (Run/Walk/Cycle), city, fromDate, toDate, includePast.
    /// An Organizer can pass mine=true to see all of their own events, past ones included.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? eventType,
        [FromQuery] string? city,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] bool includePast = false,
        [FromQuery] bool mine = false)
    {
        var query = _db.Events.AsNoTracking().AsQueryable();

        if (mine)
        {
            var userId = HttpContext.GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }
            if (HttpContext.GetCurrentRole() != "Organizer")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only Organizers have their own events." });
            }
            query = query.Where(e => e.OrganizerID == userId);
        }
        else if (!includePast)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            query = query.Where(e => e.EventDate >= today);
        }

        if (fromDate is not null)
        {
            query = query.Where(e => e.EventDate >= fromDate.Value);
        }
        if (toDate is not null)
        {
            query = query.Where(e => e.EventDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(e => e.Venue.City == city);
        }

        var events = await query.OrderBy(e => e.EventDate).Select(ToResponse).ToListAsync();
        return Ok(events);
    }

    /// <summary>Returns the full detail of one event.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvent(int id)
    {
        var result = await _db.Events.AsNoTracking()
            .Where(e => e.EventID == id)
            .Select(ToResponse)
            .FirstOrDefaultAsync();

        return result is null ? NotFound(new { message = "Event not found." }) : Ok(result);
    }

    /// <summary>Creates an event owned by the logged-in Organizer.</summary>
    [HttpPost]
    [SessionAuthorize("Organizer")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateEvent(EventRequest request)
    {
        if (request.EventDate == default)
        {
            return BadRequest(new { message = "EventDate is required." });
        }
        if (!await _db.Venues.AnyAsync(v => v.VenueID == request.VenueID))
        {
            return BadRequest(new { message = "That venue does not exist." });
        }

        var newEvent = new Event
        {
            OrganizerID = HttpContext.GetCurrentUserId()!.Value,
            VenueID = request.VenueID,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate,
            Distance = request.Distance,
            EventType = request.EventType
        };

        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync();

        var created = await _db.Events.AsNoTracking()
            .Where(e => e.EventID == newEvent.EventID)
            .Select(ToResponse)
            .FirstAsync();

        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>Updates an event. Only the Organizer who created it can do this.</summary>
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organizer")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(int id, EventRequest request)
    {
        var existing = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (existing is null)
        {
            return NotFound(new { message = "Event not found." });
        }
        if (existing.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only change your own events." });
        }
        if (request.EventDate == default)
        {
            return BadRequest(new { message = "EventDate is required." });
        }
        if (!await _db.Venues.AnyAsync(v => v.VenueID == request.VenueID))
        {
            return BadRequest(new { message = "That venue does not exist." });
        }

        existing.Name = request.Name.Trim();
        existing.Description = request.Description?.Trim();
        existing.EventDate = request.EventDate;
        existing.Distance = request.Distance;
        existing.EventType = request.EventType;
        existing.VenueID = request.VenueID;
        await _db.SaveChangesAsync();

        var updated = await _db.Events.AsNoTracking()
            .Where(e => e.EventID == id)
            .Select(ToResponse)
            .FirstAsync();

        return Ok(updated);
    }

    /// <summary>Deletes an event and its categories. Only the owner can do this, and only if nobody has enrolled.</summary>
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var existing = await _db.Events.Include(e => e.Categories).FirstOrDefaultAsync(e => e.EventID == id);
        if (existing is null)
        {
            return NotFound(new { message = "Event not found." });
        }
        if (existing.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only delete your own events." });
        }
        if (await _db.Enrollments.AnyAsync(x => x.EventID == id))
        {
            return Conflict(new { message = "This event has enrollments and cannot be deleted." });
        }

        _db.Categories.RemoveRange(existing.Categories);
        _db.Events.Remove(existing);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
