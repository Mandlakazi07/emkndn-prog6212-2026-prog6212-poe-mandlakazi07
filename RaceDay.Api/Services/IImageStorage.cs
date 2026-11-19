// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        2 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Quickstart: Azure Blob Storage client library for .NET. [online] Available at:
//              https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet [Accessed 2 November 2026].

namespace RaceDay.Api.Services;

// The API only talks to this interface. The real implementation saves to Azure Blob Storage,
// and the tests use a fake one, so tests never need Azure.
public interface IImageStorage
{
    // Saves an image and returns the public URL where it can be viewed.
    // folder    : a label for where it belongs inside the container, e.g. "events" or "profiles"
    // extension : ".jpg", ".png" or ".webp"
    Task<string> UploadAsync(Stream content, string folder, string extension, string contentType,
        CancellationToken cancellationToken = default);
}
