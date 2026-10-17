// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        17 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Integration tests in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/test/integration-tests [Accessed 17 October 2026].

using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;
using RaceDay.Tests.TestSupport;
using Xunit;

namespace RaceDay.Tests;

// Tests for event management and for the role rules that protect it
public class EventTests : ApiTestBase
{
    [Fact]
    public async Task Organizer_CanCreateAnEvent_Returns201()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);

        var response = await organizer.PostAsJsonAsync("/api/events", new
        {
            name = "Durban 10km",
            description = "Beachfront run",
            eventDate = DateString(30),
            distance = 10,
            eventType = "Run",
            venueID = venueId
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.Equal("Durban 10km", created!.Name);
        Assert.Equal("Run", created.EventType);
        Assert.Equal(venueId, created.Venue.VenueID);
    }

    [Fact]
    public async Task Organizer_CanUpdateTheirOwnEvent_Returns200()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        var ev = await CreateEventAsync(organizer, venueId);

        var response = await organizer.PutAsJsonAsync($"/api/events/{ev.EventID}", new
        {
            name = "Renamed Walk",
            description = "Updated",
            eventDate = DateString(40),
            distance = 5,
            eventType = "Walk",
            venueID = venueId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.Equal("Renamed Walk", updated!.Name);
        Assert.Equal("Walk", updated.EventType);
    }

    [Fact]
    public async Task Organizer_CanDeleteTheirOwnEvent_Returns204_AndItIsGone()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        var ev = await CreateEventAsync(organizer, venueId);

        var delete = await organizer.DeleteAsync($"/api/events/{ev.EventID}");
        var lookup = await NewClient().GetAsync($"/api/events/{ev.EventID}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, lookup.StatusCode);
    }

    [Fact]
    public async Task Organizer_CannotChangeAnotherOrganizersEvent_Returns403()
    {
        var owner = await LoggedInClientAsync("owner@test.com", "Organizer");
        var venueId = await CreateVenueAsync(owner);
        var ev = await CreateEventAsync(owner, venueId);
        var otherOrganizer = await LoggedInClientAsync("other@test.com", "Organizer");

        var update = await otherOrganizer.PutAsJsonAsync($"/api/events/{ev.EventID}", new
        {
            name = "Hijacked",
            eventDate = DateString(30),
            distance = 10,
            eventType = "Run",
            venueID = venueId
        });
        var delete = await otherOrganizer.DeleteAsync($"/api/events/{ev.EventID}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateAnEvent_Returns403()
    {
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");

        var response = await participant.PostAsJsonAsync("/api/events", new
        {
            name = "Not allowed",
            eventDate = DateString(30),
            distance = 10,
            eventType = "Run",
            venueID = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateAVenue_Returns403()
    {
        var participant = await LoggedInClientAsync("runner@test.com", "Participant");

        var response = await participant.PostAsJsonAsync("/api/venues",
            new { name = "Nope", address = "1 Street", city = "Durban" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_CannotCreateAnEvent_Returns401()
    {
        var anonymous = NewClient();

        var response = await anonymous.PostAsJsonAsync("/api/events", new
        {
            name = "Not allowed",
            eventDate = DateString(30),
            distance = 10,
            eventType = "Run",
            venueID = 1
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Anyone_CanListUpcomingEvents_WithoutLoggingIn()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        await CreateEventAsync(organizer, venueId);

        var response = await NewClient().GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await response.Content.ReadFromJsonAsync<List<EventResponse>>();
        Assert.Single(events!);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidEventType_Returns400()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);

        var response = await organizer.PostAsJsonAsync("/api/events", new
        {
            name = "Swimming gala",
            eventDate = DateString(30),
            distance = 1,
            eventType = "Swim",
            venueID = venueId
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EventList_CanBeFilteredByType()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        await CreateEventAsync(organizer, venueId); // a Run

        var runs = await NewClient().GetFromJsonAsync<List<EventResponse>>("/api/events?eventType=Run");
        var cycles = await NewClient().GetFromJsonAsync<List<EventResponse>>("/api/events?eventType=Cycle");

        Assert.Single(runs!);
        Assert.Empty(cycles!);
    }

    [Fact]
    public async Task PastEvents_AreHiddenFromTheDefaultList_ButAvailableOnRequest()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        await CreateEventAsync(organizer, venueId, daysFromToday: -5);

        var defaultList = await NewClient().GetFromJsonAsync<List<EventResponse>>("/api/events");
        var withPast = await NewClient().GetFromJsonAsync<List<EventResponse>>("/api/events?includePast=true");

        Assert.Empty(defaultList!);
        Assert.Single(withPast!);
    }
}
