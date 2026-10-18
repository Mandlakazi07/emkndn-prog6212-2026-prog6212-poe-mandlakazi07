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

// Tests for recording and viewing race results
[TestClass]
public class ResultTests : ApiTestBase
{
    // An event dated today, with a participant already enrolled in it
    private async Task<(HttpClient Organizer, HttpClient Participant, EnrollmentResponse Enrollment)> SetUpEnrolledAsync(
        int daysFromToday = 0)
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        var ev = await CreateEventAsync(organizer, venueId, daysFromToday);
        var category = await CreateCategoryAsync(organizer, ev.EventID);
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");

        var enrolled = await participant.PostAsJsonAsync("/api/enrollments",
            new { eventID = ev.EventID, categoryID = category.CategoryID });
        enrolled.EnsureSuccessStatusCode();
        var enrollment = await enrolled.Content.ReadFromJsonAsync<EnrollmentResponse>();

        return (organizer, participant, enrollment!);
    }

    private static Task<HttpResponseMessage> RecordResultAsync(HttpClient client, int enrollmentId) =>
        client.PostAsJsonAsync("/api/results", new { enrollmentID = enrollmentId, finishTime = "01:45:32", finishPosition = 47 });

    [TestMethod]
    public async Task Organizer_CanRecordAResult_AndTheParticipantSeesIt()
    {
        var (organizer, participant, enrollment) = await SetUpEnrolledAsync();

        var response = await RecordResultAsync(organizer, enrollment.EnrollmentID);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var mine = await participant.GetFromJsonAsync<List<ResultResponse>>("/api/results/me");
        Assert.AreEqual(1, mine!.Count);
        var result = mine[0];
        Assert.AreEqual<int?>(47, result.FinishPosition);
        Assert.AreEqual<TimeSpan?>(new TimeSpan(1, 45, 32), result.FinishTime);
        Assert.AreEqual(1, result.TotalFinishers);
        Assert.AreEqual("Test Run", result.EventName);
    }

    [TestMethod]
    public async Task Participant_CannotRecordAResult_Returns403()
    {
        var (_, participant, enrollment) = await SetUpEnrolledAsync();

        var response = await RecordResultAsync(participant, enrollment.EnrollmentID);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Result_CannotBeRecordedBeforeTheEventHasHappened_Returns400()
    {
        var (organizer, _, enrollment) = await SetUpEnrolledAsync(daysFromToday: 30);

        var response = await RecordResultAsync(organizer, enrollment.EnrollmentID);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Result_CannotBeRecordedTwiceForTheSameEnrollment_Returns409()
    {
        var (organizer, _, enrollment) = await SetUpEnrolledAsync();
        await RecordResultAsync(organizer, enrollment.EnrollmentID);

        var second = await RecordResultAsync(organizer, enrollment.EnrollmentID);

        Assert.AreEqual(HttpStatusCode.Conflict, second.StatusCode);
    }

    [TestMethod]
    public async Task Unauthenticated_CannotViewResults_Returns401()
    {
        var response = await NewClient().GetAsync("/api/results/me");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
