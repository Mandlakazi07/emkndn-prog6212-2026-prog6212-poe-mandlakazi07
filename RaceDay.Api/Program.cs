// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Session and state management in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/app-state [Accessed 16 October 2026].
//              Microsoft Learn, n.d. Get started with Swashbuckle and ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/tutorials/getting-started-with-swashbuckle [Accessed 16 October 2026].

using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Register the EF Core DbContext using the connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session support: the server remembers who is logged in (UserID and Role) between requests.
// The browser only holds a session ID cookie; the real data stays on the server.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

// Session must run before authorization and the controllers that read it
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
