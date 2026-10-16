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
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly AppDbContext _db;

    public VenuesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Lists all venues, so an Organizer can pick one when creating an event.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VenueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVenues()
    {
        var venues = await _db.Venues.AsNoTracking()
            .OrderBy(v => v.Name)
            .Select(v => new VenueResponse { VenueID = v.VenueID, Name = v.Name, Address = v.Address, City = v.City })
            .ToListAsync();

        return Ok(venues);
    }

    /// <summary>Creates a new venue. Organizers only.</summary>
    [HttpPost]
    [SessionAuthorize("Organizer")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateVenue(VenueRequest request)
    {
        var venue = new Venue
        {
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim()
        };

        _db.Venues.Add(venue);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created,
            new VenueResponse { VenueID = venue.VenueID, Name = venue.Name, Address = venue.Address, City = venue.City });
    }
}
