// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        17 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Integration tests in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/test/integration-tests [Accessed 17 October 2026].

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Tests.TestSupport;

// Starts the real RaceDay API inside the test process, but swaps SQL Server for an in-memory database,
// so tests are fast, need no SQL Server, and also run on the CI server.
// AppDbContext is only used to point the factory at the RaceDay.Api assembly.
public class RaceDayFactory : WebApplicationFactory<AppDbContext>
{
    // A unique name per factory means every test starts with an empty database
    private readonly string _databaseName = "RaceDayTests-" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the SQL Server database registration made in Program.cs
            var sqlServerRegistrations = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration") &&
                 d.ServiceType.GetGenericArguments().Contains(typeof(AppDbContext)))).ToList();

            foreach (var registration in sqlServerRegistrations)
            {
                services.Remove(registration);
            }

            // Register the in-memory database instead
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            // Replace Azure Blob Storage with a fake, so tests never need Azure
            services.RemoveAll<IImageStorage>();
            services.AddSingleton<IImageStorage, FakeImageStorage>();
        });
    }
}
