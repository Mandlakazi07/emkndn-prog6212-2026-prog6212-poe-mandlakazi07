// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        2 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Upload files in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads [Accessed 2 November 2026].

namespace RaceDay.Api.Dtos;

// What the API returns after an image has been uploaded: where it can be viewed
public class ImageUploadResponse
{
    public string ImageUrl { get; set; } = string.Empty;
}
