using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace backend.IntegrationTests.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<MottuContext>>();
            services.RemoveAll<MottuContext>();

            services.AddDbContext<MottuContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDatabase");
            });

            services.RemoveAll(typeof(IHostedService));
            services.AddHostedService<MqttConsumerService>(provider => null!);

            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<MottuContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

                    context.Database.EnsureCreated();
                    SeedDatabase(context, userManager, roleManager).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding database: {ex.Message}");
                }
            }
        });
    }

    private static async Task SeedDatabase(
        MottuContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole<int>("User"));

        if (await userManager.FindByEmailAsync("testuser@test.com") == null)
        {
            var testUser = new ApplicationUser
            {
                UserName = "testuser@test.com",
                Email = "testuser@test.com",
                Name = "Test User",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(testUser, "Test@123");
            await userManager.AddToRoleAsync(testUser, "User");
        }

        if (!context.UwbAnchors.Any())
        {
            context.UwbAnchors.AddRange(
                new UwbAnchor { Name = "A1", X = 0.0, Y = 0.0, Z = 2.0 },
                new UwbAnchor { Name = "A2", X = 10.0, Y = 0.0, Z = 2.0 },
                new UwbAnchor { Name = "A3", X = 10.0, Y = 10.0, Z = 2.0 },
                new UwbAnchor { Name = "A4", X = 0.0, Y = 10.0, Z = 2.0 }
            );

            await context.SaveChangesAsync();
        }
    }
}
