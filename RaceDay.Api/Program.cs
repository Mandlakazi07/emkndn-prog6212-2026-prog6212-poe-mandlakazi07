// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        17 October 2026
// AI use:     Deepseek helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Use cookie authentication without ASP.NET Core Identity. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/security/authentication/cookie [Accessed 17 October 2026].
//              Adnan, Y., n.d. Authentication_API_With_UnitTesting. [online] GitHub. Available at:
//              https://github.com/YusraAdnan/Authentication_API_With_UnitTesting [Accessed 17 October 2026].
//              Microsoft Learn, n.d. Get started with Swashbuckle and ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/tutorials/getting-started-with-swashbuckle [Accessed 16 October 2026].

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Register the EF Core DbContext using the connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie authentication: when a user logs in, the API creates an encrypted cookie holding who they are
// (their ID, email and role). The browser sends that cookie back with every request.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RaceDay.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        // This is an API, so it answers with status codes instead of redirecting to a login page

        // Not logged in -> 401 Unauthorized
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        // Logged in but the wrong role -> 403 Forbidden
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// Image storage: the API saves uploaded images to Azure Blob Storage through this service
builder.Services.AddSingleton<IImageStorage, BlobImageStorage>();

// Add services to the container.
builder.Services.AddControllers();
// Built-in OpenAPI: generates the machine-readable description of every endpoint
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Swagger UI: an interactive page for reading and testing every endpoint in the browser
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "RaceDay API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Two guards that run on every request before it reaches a controller:
app.UseAuthentication(); // Who is this person? (reads the login cookie)
app.UseAuthorization();  // What are they allowed to do? (checks [Authorize] and roles)

app.MapControllers();

app.Run();
