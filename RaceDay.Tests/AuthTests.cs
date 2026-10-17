// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        17 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Integration tests in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/test/integration-tests [Accessed 17 October 2026].

using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Tests.TestSupport;
using Xunit;

namespace RaceDay.Tests;

// Tests for registration, login, logout and the session (success and failure cases)
public class AuthTests : ApiTestBase
{
    [Fact]
    public async Task Register_WithValidDetails_Returns201_AndNeverReturnsThePassword()
    {
        var client = NewClient();

        var response = await RegisterAsync(client, "organizer@test.com", "Organizer");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("organizer@test.com", body);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var client = NewClient();
        await RegisterAsync(client, "same@test.com", "Participant");

        // The same email in capital letters is still the same account
        var second = await RegisterAsync(client, "SAME@test.com", "Participant");

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidRole_Returns400()
    {
        var client = NewClient();

        var response = await RegisterAsync(client, "admin@test.com", "Admin");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortPassword_Returns400()
    {
        var client = NewClient();

        var response = await RegisterAsync(client, "short@test.com", "Participant", password: "short");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Passwords_AreStoredAsBCryptHashes_NeverAsPlainText()
    {
        var client = NewClient();
        await RegisterAsync(client, "hash@test.com", "Participant");

        // Look directly in the database
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == "hash@test.com");

        Assert.NotEqual(Password, user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_Returns200_AndStartsASession()
    {
        var client = NewClient();
        await RegisterAsync(client, "login@test.com", "Organizer");

        var login = await LoginAsync(client, "login@test.com");
        var me = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var profile = await me.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("login@test.com", profile!.Email);
        Assert.Equal("Organizer", profile.Role);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = NewClient();
        await RegisterAsync(client, "wrong@test.com", "Participant");

        var response = await LoginAsync(client, "wrong@test.com", "NotTheRightPassword1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var client = NewClient();

        var response = await LoginAsync(client, "nobody@test.com");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_EndsTheSession()
    {
        var client = await LoggedInClientAsync("logout@test.com", "Participant");

        var logout = await client.PostAsync("/api/auth/logout", null);
        var meAfterLogout = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutLogin_Returns401()
    {
        var client = NewClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
