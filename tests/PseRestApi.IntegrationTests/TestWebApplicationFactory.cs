using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Infrastructure.Persistence;

namespace PseRestApi.IntegrationTests;

/// <summary>
/// Custom <see cref="WebApplicationFactory{T}"/> that configures the host for integration testing.
/// </summary>
/// <remarks>
/// Replaces the production PostgreSQL database with an in-memory SQLite database so tests
/// can run without an external database server. Override <c>ConfigureServices</c> further
/// to mock external HTTP clients or swap other services.
/// </remarks>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register an in-memory SQLite database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("PseRestApiTestDb");
            });
        });
    }
}
