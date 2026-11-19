// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        3 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 3 November 2026].
//              Microsoft Learn, n.d. Make HTTP requests using IHttpClientFactory in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/http-requests [Accessed 3 November 2026].
//              Adnan, Y., n.d. Authentication_API_With_UnitTesting. [online] GitHub. Available at:
//              https://github.com/YusraAdnan/Authentication_API_With_UnitTesting [Accessed 3 November 2026].

using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Login state. When someone logs in, the web app creates its OWN encrypted cookie holding their ID, name, role,
// and the API's login cookie. This web app's cookie has a different name from the API's, because both run on localhost.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RaceDay.Web.Auth";
        options.Cookie.HttpOnly = true;
        options.LoginPath = "/Account/Login";            // not logged in -> sent here
        options.AccessDeniedPath = "/Account/AccessDenied"; // logged in with the wrong role -> sent here
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // same lifetime as the API's login cookie
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization();

// The API client. The web app talks to the API ONLY through this typed HttpClient.
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiCookieHandler>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5159/";

builder.Services.AddHttpClient<RaceDayApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(15);
    })
    // IMPORTANT: handlers are shared between users, so we switch the built-in cookie jar OFF.
    // Otherwise one user's API login cookie could be sent on another user's requests.
    // Instead, ApiCookieHandler adds the right user's cookie to each request by hand.
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false })
    .AddHttpMessageHandler<ApiCookieHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // A friendly error page instead of a crash screen
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Two guards that run on every request:
app.UseAuthentication(); // Who is this person? (reads the login cookie)
app.UseAuthorization();  // What are they allowed to see? (checks [Authorize] and roles)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
