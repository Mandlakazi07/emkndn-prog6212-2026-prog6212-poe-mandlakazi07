// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Adnan, Y., n.d. Authentication_API_With_UnitTesting. [online] GitHub. Available at:
//              https://github.com/YusraAdnan/Authentication_API_With_UnitTesting [Accessed 17 November 2026].

using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceDay.Api.Dtos;
using RaceDay.Tests.TestSupport;

namespace RaceDay.Tests;

// Tests for the optional age range and distance on a category (added after the Part 2 feedback)
[TestClass]
public class CategoryFieldTests : ApiTestBase
{
    private async Task<(HttpClient Organizer, EventResponse Event)> SetUpEventAsync()
    {
        var organizer = await LoggedInClientAsync("org@test.com", "Organizer");
        var venueId = await CreateVenueAsync(organizer);
        var ev = await CreateEventAsync(organizer, venueId);
        return (organizer, ev);
    }

    [TestMethod]
    public async Task Organizer_CanCreateACategory_WithAnAgeRangeAndDistance()
    {
        var (organizer, ev) = await SetUpEventAsync();

        var response = await organizer.PostAsJsonAsync($"/api/events/{ev.EventID}/categories",
            new { categoryName = "Senior", minAge = 20, maxAge = 39, distanceKm = 10 });

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.AreEqual<int?>(20, created!.MinAge);
        Assert.AreEqual<int?>(39, created.MaxAge);
        Assert.AreEqual<decimal?>(10m, created.DistanceKm);

        // The values are saved, and returned when the categories are listed
        var list = await NewClient().GetFromJsonAsync<List<CategoryResponse>>($"/api/events/{ev.EventID}/categories");
        Assert.AreEqual(1, list!.Count);
        Assert.AreEqual<int?>(39, list[0].MaxAge);
    }

    [TestMethod]
    public async Task Category_WithMinAgeGreaterThanMaxAge_Returns400()
    {
        var (organizer, ev) = await SetUpEventAsync();

        var response = await organizer.PostAsJsonAsync($"/api/events/{ev.EventID}/categories",
            new { categoryName = "Impossible", minAge = 50, maxAge = 20 });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Organizer_CanUpdateACategorysAgeRangeAndDistance()
    {
        var (organizer, ev) = await SetUpEventAsync();
        var category = await CreateCategoryAsync(organizer, ev.EventID);

        var response = await organizer.PutAsJsonAsync($"/api/categories/{category.CategoryID}",
            new { categoryName = "Masters", minAge = 40, maxAge = 99, distanceKm = 21.1 });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.AreEqual("Masters", updated!.CategoryName);
        Assert.AreEqual<int?>(40, updated.MinAge);
        Assert.AreEqual<decimal?>(21.1m, updated.DistanceKm);
    }
}
