// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, 2023. Introduction to relationships - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/relationships [Accessed 16 October 2026].

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    private static CategoryResponse ToResponse(Category c) => new()
    {
        CategoryID = c.CategoryID,
        EventID = c.EventID,
        CategoryName = c.CategoryName
    };

    /// <summary>Lists the categories available for one event.</summary>
    [HttpGet("events/{eventId:int}/categories")]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        var categories = await _db.Categories.AsNoTracking()
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryResponse { CategoryID = c.CategoryID, EventID = c.EventID, CategoryName = c.CategoryName })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>Adds a category to an event. Only the event's Organizer can do this.</summary>
    [HttpPost("events/{eventId:int}/categories")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCategory(int eventId, CategoryRequest request)
    {
        var parentEvent = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (parentEvent is null)
        {
            return NotFound(new { message = "Event not found." });
        }
        if (parentEvent.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only add categories to your own events." });
        }

        var category = new Category { EventID = eventId, CategoryName = request.CategoryName.Trim() };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResponse(category));
    }

    /// <summary>Renames a category. Only the event's Organizer can do this.</summary>
    [HttpPut("categories/{id:int}")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }
        if (category.Event.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only change categories on your own events." });
        }

        category.CategoryName = request.CategoryName.Trim();
        await _db.SaveChangesAsync();

        return Ok(ToResponse(category));
    }

    /// <summary>Deletes a category. Only the event's Organizer can do this, and only if nobody chose it.</summary>
    [HttpDelete("categories/{id:int}")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }
        if (category.Event.OrganizerID != HttpContext.GetCurrentUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only delete categories on your own events." });
        }
        if (await _db.Enrollments.AnyAsync(x => x.CategoryID == id))
        {
            return Conflict(new { message = "Participants have enrolled in this category, so it cannot be deleted." });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
