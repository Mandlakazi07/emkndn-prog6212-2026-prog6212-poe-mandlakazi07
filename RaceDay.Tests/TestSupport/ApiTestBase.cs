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

using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceDay.Api.Dtos;

namespace RaceDay.Tests.TestSupport;

// Shared helpers for all API tests. MSTest creates a new instance of the test class for every test,
// so each test gets its own factory and therefore its own empty database.
[TestClass]
public abstract class ApiTestBase
{
    protected const string Password = "Passw0rd!123";

    protected readonly RaceDayFactory Factory = new();

    // A new client is a new "browser" with its own login cookie
    protected HttpClient NewClient() => Factory.CreateClient();

    // A date relative to today in the format the API expects (0 = today, negative = past)
    protected static string DateString(int daysFromToday) =>
        DateOnly.FromDateTime(DateTime.Today.AddDays(daysFromToday)).ToString("yyyy-MM-dd");

    protected static Task<HttpResponseMessage> RegisterAsync(
        HttpClient client, string email, string role, string fullName = "Test User", string password = Password) =>
        client.PostAsJsonAsync("/api/auth/register", new { fullName, email, password, role });

    protected static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password = Password) =>
        client.PostAsJsonAsync("/api/auth/login", new { email, password });

    // Registers a new user, logs them in, and returns a client that is now logged in
    protected async Task<HttpClient> LoggedInClientAsync(string email, string role)
    {
        var client = NewClient();
        (await RegisterAsync(client, email, role)).EnsureSuccessStatusCode();
        (await LoginAsync(client, email)).EnsureSuccessStatusCode();
        return client;
    }

    protected static async Task<int> CreateVenueAsync(HttpClient organizer)
    {
        var response = await organizer.PostAsJsonAsync("/api/venues",
            new { name = "Kings Park", address = "2 Jacko Jackson Drive", city = "Durban" });
        response.EnsureSuccessStatusCode();
        var venue = await response.Content.ReadFromJsonAsync<VenueResponse>();
        return venue!.VenueID;
    }

    protected static async Task<EventResponse> CreateEventAsync(
        HttpClient organizer, int venueId, int daysFromToday = 30, string name = "Test Run")
    {
        var response = await organizer.PostAsJsonAsync("/api/events", new
        {
            name,
            description = "A test event",
            eventDate = DateString(daysFromToday),
            distance = 10,
            eventType = "Run",
            venueID = venueId
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EventResponse>())!;
    }

    protected static async Task<CategoryResponse> CreateCategoryAsync(
        HttpClient organizer, int eventId, string name = "10km")
    {
        var response = await organizer.PostAsJsonAsync($"/api/events/{eventId}/categories", new { categoryName = name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryResponse>())!;
    }

    // Runs after every test: shuts the in-memory API down
    [TestCleanup]
    public void Cleanup()
    {
        Factory.Dispose();
    }
}
