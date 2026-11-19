// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        2 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Upload files in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads [Accessed 2 November 2026].

namespace RaceDay.Api.Services;

// What a valid upload looks like once it has been checked
public sealed record ValidImage(string Extension, string ContentType);

// Checks an uploaded file really is a small JPEG, PNG or WebP picture.
// We do not trust the file name or the content type the browser claims: we read the first bytes of the file,
// which identify the real format, and make sure they agree with the claimed type.
public static class ImageValidator
{
    public const long MaxBytes = 2 * 1024 * 1024; // 2 MB

    public static async Task<ValidImage?> CheckAsync(IFormFile file)
    {
        if (file.Length == 0 || file.Length > MaxBytes)
        {
            return null;
        }

        // Read the first 12 bytes
        var header = new byte[12];
        await using (var stream = file.OpenReadStream())
        {
            var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
            if (read < header.Length)
            {
                return null;
            }
        }

        var declared = (file.ContentType ?? string.Empty).ToLowerInvariant();

        var isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                    && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
        var isWebp = header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46   // "RIFF"
                     && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50; // "WEBP"

        if (isJpeg && declared == "image/jpeg") return new ValidImage(".jpg", "image/jpeg");
        if (isPng && declared == "image/png") return new ValidImage(".png", "image/png");
        if (isWebp && declared == "image/webp") return new ValidImage(".webp", "image/webp");

        return null;
    }
}
