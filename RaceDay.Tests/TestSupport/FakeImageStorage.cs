// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        2 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Adnan, Y., n.d. Authentication_API_With_UnitTesting. [online] GitHub. Available at:
//              https://github.com/YusraAdnan/Authentication_API_With_UnitTesting [Accessed 17 October 2026].
//              Microsoft Learn, n.d. Integration tests in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/test/integration-tests [Accessed 17 October 2026].

using RaceDay.Api.Services;

namespace RaceDay.Tests.TestSupport;

// A stand-in for Azure Blob Storage used only by the tests: it saves nothing and just returns a made-up URL.
// This lets the upload endpoints be tested without an Azure account or internet.
public class FakeImageStorage : IImageStorage
{
    public Task<string> UploadAsync(Stream content, string folder, string extension, string contentType,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"https://fake-storage.test/{folder}/{Guid.NewGuid():N}{extension}");
    }
}
