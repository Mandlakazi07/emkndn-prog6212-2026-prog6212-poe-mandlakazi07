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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Tests.TestSupport;

namespace RaceDay.Tests;

// Tests for registration, login, logout and the login cookie (success and failure cases)
[TestClass]
public class AuthTests : ApiTestBase
{
    [TestMethod]
    public async Task Register_WithValidDetails_Returns201_AndNeverReturnsThePassword()
    {
        var client = NewClient();

        var response = await RegisterAsync(client, "organizer@test.com", "Organizer");

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(body.Contains("organizer@test.com"));
        Assert.IsFalse(body.Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var client = NewClient();
        await RegisterAsync(client, "same@test.com", "Participant");

        // The same email in capital letters is still the same account
        var second = await RegisterAsync(client, "SAME@test.com", "Participant");

        Assert.AreEqual(HttpStatusCode.Conflict, second.StatusCode);
    }

    [TestMethod]
    public async Task Register_WithInvalidRole_Returns400()
    {
        var client = NewClient();

        var response = await RegisterAsync(client, "admin@test.com", "Admin");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Register_WithShortPassword_Returns400()
    {
        var client = NewClient();

        var response = await RegisterAsync(client, "short@test.com", "Participant", password: "short");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Passwords_AreStoredAsBCryptHashes_NeverAsPlainText()
    {
        var client = NewClient();
        await RegisterAsync(client, "hash@test.com", "Participant");

        // Look directly in the database
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == "hash@test.com");

        Assert.AreNotEqual(Password, user.PasswordHash);
        Assert.IsTrue(user.PasswordHash.StartsWith("$2", StringComparison.Ordinal));
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));
    }

    [TestMethod]
    public async Task Login_WithCorrectCredentials_Returns200_AndSignsTheUserIn()
    {
        var client = NewClient();
        await RegisterAsync(client, "login@test.com", "Organizer");

        var login = await LoginAsync(client, "login@test.com");
        var me = await client.GetAsync("/api/users/me");

        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, me.StatusCode);
        var profile = await me.Content.ReadFromJsonAsync<UserResponse>();
        Assert.AreEqual("login@test.com", profile!.Email);
        Assert.AreEqual("Organizer", profile.Role);
    }

    [TestMethod]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = NewClient();
        await RegisterAsync(client, "wrong@test.com", "Participant");

        var response = await LoginAsync(client, "wrong@test.com", "NotTheRightPassword1");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var client = NewClient();

        var response = await LoginAsync(client, "nobody@test.com");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Logout_RemovesTheLoginCookie()
    {
        var client = await LoggedInClientAsync("logout@test.com", "Participant");

        var logout = await client.PostAsync("/api/auth/logout", null);
        var meAfterLogout = await client.GetAsync("/api/users/me");

        Assert.AreEqual(HttpStatusCode.OK, logout.StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
    }

    [TestMethod]
    public async Task ProtectedEndpoint_WithoutLogin_Returns401()
    {
        var client = NewClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
