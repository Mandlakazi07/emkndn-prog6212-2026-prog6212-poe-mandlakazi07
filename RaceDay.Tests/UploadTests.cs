// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        2 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Adnan, Y., n.d. Authentication_API_With_UnitTesting. [online] GitHub. Available at:
//              https://github.com/YusraAdnan/Authentication_API_With_UnitTesting [Accessed 17 October 2026].
//              Microsoft Learn, n.d. Integration tests in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/test/integration-tests [Accessed 17 October 2026].

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceDay.Api.Dtos;
using RaceDay.Tests.TestSupport;

namespace RaceDay.Tests;

// Tests for the banner and profile picture upload endpoints (the image storage is a fake, so no Azure is needed)
[TestClass]
public class UploadTests : ApiTestBase
{
    // The first bytes of a real PNG file (the validator checks these, not the file name)
    private static readonly byte[] PngBytes =
    {
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    private static MultipartFormDataContent ImageForm(byte[] bytes, string contentType, string fileName = "photo.png")
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", fileName } };
    }

    // An Organizer who owns one event
    private async Task<(HttpClient Organizer, EventResponse Event)> SetUpEventAsync()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        var ev = await CreateEventAsync(organizer, venueId);
        return (organizer, ev);
    }

    [TestMethod]
    public async Task Organizer_CanUploadABanner_ForTheirOwnEvent()
    {
        var (organizer, ev) = await SetUpEventAsync();

        var response = await organizer.PostAsync($"/api/events/{ev.EventID}/banner", ImageForm(PngBytes, "image/png"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var uploaded = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
        Assert.IsTrue(uploaded!.ImageUrl.StartsWith("https://fake-storage.test/events/", StringComparison.Ordinal));

        // The URL is saved on the event and returned by the public event endpoint
        var lookup = await NewClient().GetFromJsonAsync<EventResponse>($"/api/events/{ev.EventID}");
        Assert.AreEqual(uploaded.ImageUrl, lookup!.BannerImageUrl);
    }

    [TestMethod]
    public async Task Participant_CannotUploadABanner_Returns403()
    {
        var (_, ev) = await SetUpEventAsync();
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");

        var response = await participant.PostAsync($"/api/events/{ev.EventID}/banner", ImageForm(PngBytes, "image/png"));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Organizer_CannotUploadABannerForAnotherOrganizersEvent_Returns403()
    {
        var (_, ev) = await SetUpEventAsync();
        var otherOrganizer = await LoggedInClientAsync("other@test.com", "Organizer");

        var response = await otherOrganizer.PostAsync($"/api/events/{ev.EventID}/banner", ImageForm(PngBytes, "image/png"));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Banner_ThatIsNotARealImage_Returns400()
    {
        var (organizer, ev) = await SetUpEventAsync();
        var notAnImage = Encoding.UTF8.GetBytes("this is definitely not a picture");

        // It claims to be a PNG, but the bytes say otherwise
        var response = await organizer.PostAsync($"/api/events/{ev.EventID}/banner", ImageForm(notAnImage, "image/png"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Banner_WithTheWrongContentType_Returns400()
    {
        var (organizer, ev) = await SetUpEventAsync();

        // Real PNG bytes, but sent as plain text
        var response = await organizer.PostAsync($"/api/events/{ev.EventID}/banner", ImageForm(PngBytes, "text/plain"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Banner_WithoutLogin_Returns401()
    {
        var (_, ev) = await SetUpEventAsync();

        var response = await NewClient().PostAsync($"/api/events/{ev.EventID}/banner", ImageForm(PngBytes, "image/png"));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task LoggedInUser_CanUploadAProfilePicture_AndItAppearsOnTheirProfile()
    {
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");

        var response = await participant.PostAsync("/api/users/me/profile-picture", ImageForm(PngBytes, "image/png"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var uploaded = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
        Assert.IsTrue(uploaded!.ImageUrl.StartsWith("https://fake-storage.test/profiles/", StringComparison.Ordinal));

        var me = await participant.GetFromJsonAsync<UserResponse>("/api/users/me");
        Assert.AreEqual(uploaded.ImageUrl, me!.ProfilePictureUrl);
    }

    [TestMethod]
    public async Task ProfilePicture_WithoutLogin_Returns401()
    {
        var response = await NewClient().PostAsync("/api/users/me/profile-picture", ImageForm(PngBytes, "image/png"));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
