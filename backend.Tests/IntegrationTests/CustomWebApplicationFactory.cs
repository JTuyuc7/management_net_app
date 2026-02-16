using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using backend.Data;

namespace backend.Tests.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// Configures a test server with an in-memory database
/// Each test class gets its own database instance, but tests within the same class share it
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "IntegrationTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TaskDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a new DbContext using an in-memory database for testing
            // Use the same database name for all tests in this test class
            services.AddDbContext<TaskDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TaskDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            }
        });

        builder.UseEnvironment("Testing");
    }
}
