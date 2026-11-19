// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 3 November 2026].

namespace RaceDay.Web.Models.Api;

// An event as the API describes it, including its venue and categories
public class EventDto
{
    public int EventID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }
    public int OrganizerID { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public VenueDto Venue { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}
