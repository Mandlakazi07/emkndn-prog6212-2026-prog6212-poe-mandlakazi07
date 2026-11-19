// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 17 November 2026].

namespace RaceDay.Web.Models.Api;

// A category (for example "Senior, 20 to 39, 10 km") inside an event
public class CategoryDto
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? DistanceKm { get; set; }

    // A short readable description, for example "20-39 years | 10 km"
    public string Summary
    {
        get
        {
            var parts = new List<string>();
            if (MinAge.HasValue && MaxAge.HasValue) parts.Add($"{MinAge}-{MaxAge} years");
            else if (MinAge.HasValue) parts.Add($"{MinAge}+ years");
            else if (MaxAge.HasValue) parts.Add($"up to {MaxAge} years");
            if (DistanceKm.HasValue) parts.Add($"{DistanceKm.Value:0.##} km");
            return parts.Count == 0 ? "Open to all" : string.Join(" | ", parts);
        }
    }
}
