using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;

namespace CardiacPatientMonitoringSystem.Tests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the real SQL Server AppDbContext registration.
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<
                IDbContextOptionsConfiguration<AppDbContext>
            >();

            // Register an isolated in-memory database
            // specifically for integration tests.
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(
                    "CardiacPatientMonitoringTestDb"
                );
            });

            var serviceProvider =
                services.BuildServiceProvider();

            using var scope =
                serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            // Start every integration test environment
            // with a clean database.
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed patient used by PatientsApiTests.
            context.Patients.Add(
                new Patient
                {
                    Id = 1001,
                    UserId = "integration-test-user",
                    FullName = "Ahmad Khalil",
                    DateOfBirth = new DateTime(1985, 6, 15),
                    Gender = "Male"
                }
            );

            // Seed medication used by MedicationsApiTests.
            context.Medications.Add(
                new Medication
                {
                    Id = 1001,
                    Name = "Aspirin",
                    Description = "Low-dose aspirin"
                }
            );

            context.SaveChanges();
        });
    }
}