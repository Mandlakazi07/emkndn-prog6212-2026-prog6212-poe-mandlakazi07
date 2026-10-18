// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        18 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Adnan, Y., n.d. Unit testing with the Auth API [lecture notes]. PROG6212, The Independent Institute of Education (IIE).
//              Adnan, Y., n.d. Authentication_API_With_UnitTesting. [online] GitHub. Available at:
//              https://github.com/YusraAdnan/Authentication_API_With_UnitTesting [Accessed 17 October 2026].
//              Microsoft Learn, n.d. Integration tests in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/test/integration-tests [Accessed 17 October 2026].

using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceDay.Api.Dtos;
using RaceDay.Tests.TestSupport;

namespace RaceDay.Tests;

// Tests for enrolling in events: who can enrol, what is recorded, and what is refused
[TestClass]
public class EnrollmentTests : ApiTestBase
{
    // An Organizer with one event (and one category in it) ready to be entered
    private async Task<(HttpClient Organizer, EventResponse Event, CategoryResponse Category)> SetUpEventAsync(
        int daysFromToday = 30)
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        var ev = await CreateEventAsync(organizer, venueId, daysFromToday);
        var category = await CreateCategoryAsync(organizer, ev.EventID);
        return (organizer, ev, category);
    }

    private static Task<HttpResponseMessage> EnrollAsync(HttpClient participant, int eventId, int categoryId) =>
        participant.PostAsJsonAsync("/api/enrollments", new { eventID = eventId, categoryID = categoryId });

    [TestMethod]
    public async Task Participant_CanEnrol_AndTheEnrollmentIsRecordedAsPending()
    {
        var (organizer, ev, category) = await SetUpEventAsync();
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");

        var response = await EnrollAsync(participant, ev.EventID, category.CategoryID);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EnrollmentResponse>();
        Assert.AreEqual("Pending", created!.Status);

        // The participant sees it in their own list, linked to the right event and category
        var mine = await participant.GetFromJsonAsync<List<EnrollmentResponse>>("/api/enrollments/me");
        Assert.AreEqual(1, mine!.Count);
        Assert.AreEqual(ev.EventID, mine[0].EventID);
        Assert.AreEqual(category.CategoryID, mine[0].CategoryID);

        // The Organizer sees it in the list for their event
        var forOrganizer = await organizer.GetFromJsonAsync<List<EnrollmentResponse>>(
            $"/api/events/{ev.EventID}/enrollments");
        Assert.AreEqual(1, forOrganizer!.Count);
        Assert.AreEqual("Test User", forOrganizer[0].ParticipantName);
        Assert.AreEqual("10km", forOrganizer[0].CategoryName);
    }

    [TestMethod]
    public async Task Participant_CannotEnrolTwiceInTheSameEvent_Returns409()
    {
        var (_, ev, category) = await SetUpEventAsync();
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");
        await EnrollAsync(participant, ev.EventID, category.CategoryID);

        var second = await EnrollAsync(participant, ev.EventID, category.CategoryID);

        Assert.AreEqual(HttpStatusCode.Conflict, second.StatusCode);
    }

    [TestMethod]
    public async Task Organizer_CannotEnrol_Returns403()
    {
        var (organizer, ev, category) = await SetUpEventAsync();

        var response = await EnrollAsync(organizer, ev.EventID, category.CategoryID);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Unauthenticated_CannotEnrol_Returns401()
    {
        var (_, ev, category) = await SetUpEventAsync();

        var response = await EnrollAsync(NewClient(), ev.EventID, category.CategoryID);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Enrol_WithACategoryFromAnotherEvent_Returns400()
    {
        var (organizer, ev, _) = await SetUpEventAsync();
        var secondEvent = await CreateEventAsync(organizer, ev.Venue.VenueID, 40, "Second Run");
        var otherCategory = await CreateCategoryAsync(organizer, secondEvent.EventID, "21km");
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");

        var response = await EnrollAsync(participant, ev.EventID, otherCategory.CategoryID);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Enrol_InAnEventThatAlreadyHappened_Returns400()
    {
        var (_, ev, category) = await SetUpEventAsync(daysFromToday: -5);
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");

        var response = await EnrollAsync(participant, ev.EventID, category.CategoryID);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Organizer_CanConfirmAnEnrollment_AndTheParticipantSeesIt()
    {
        var (organizer, ev, category) = await SetUpEventAsync();
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");
        var enrollment = await (await EnrollAsync(participant, ev.EventID, category.CategoryID))
            .Content.ReadFromJsonAsync<EnrollmentResponse>();

        var confirm = await organizer.PutAsJsonAsync(
            $"/api/enrollments/{enrollment!.EnrollmentID}/status", new { status = "Confirmed" });

        Assert.AreEqual(HttpStatusCode.OK, confirm.StatusCode);
        var mine = await participant.GetFromJsonAsync<List<EnrollmentResponse>>("/api/enrollments/me");
        Assert.AreEqual(1, mine!.Count);
        Assert.AreEqual("Confirmed", mine[0].Status);
    }

    [TestMethod]
    public async Task Participant_CannotConfirmAnEnrollment_Returns403()
    {
        var (_, ev, category) = await SetUpEventAsync();
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");
        var enrollment = await (await EnrollAsync(participant, ev.EventID, category.CategoryID))
            .Content.ReadFromJsonAsync<EnrollmentResponse>();

        var response = await participant.PutAsJsonAsync(
            $"/api/enrollments/{enrollment!.EnrollmentID}/status", new { status = "Confirmed" });

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Organizer_CannotSeeEnrollmentsForAnotherOrganizersEvent_Returns403()
    {
        var (_, ev, _) = await SetUpEventAsync();
        var otherOrganizer = await LoggedInClientAsync("other@test.com", "Organizer");

        var response = await otherOrganizer.GetAsync($"/api/events/{ev.EventID}/enrollments");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Participant_CanWithdrawTheirOwnEnrollment_Returns204()
    {
        var (_, ev, category) = await SetUpEventAsync();
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");
        var enrollment = await (await EnrollAsync(participant, ev.EventID, category.CategoryID))
            .Content.ReadFromJsonAsync<EnrollmentResponse>();

        var delete = await participant.DeleteAsync($"/api/enrollments/{enrollment!.EnrollmentID}");

        Assert.AreEqual(HttpStatusCode.NoContent, delete.StatusCode);
        var mine = await participant.GetFromJsonAsync<List<EnrollmentResponse>>("/api/enrollments/me");
        Assert.AreEqual(0, mine!.Count);
    }
}
