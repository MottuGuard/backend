using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public static class DbInitiliazer
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            string[] roles = { "Admin", "Supervisor", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var identityRole = new IdentityRole<int>();
                    identityRole.Name = role;
                    identityRole.NormalizedName = role.ToUpper();
                    await roleManager.CreateAsync(identityRole);
                }
            }
        }

        public static async Task SeedAnchorsAsync(MottuContext context)
        {
            if (!await context.UwbAnchors.AnyAsync())
            {
                var anchors = new[]
                {
                    new UwbAnchor { Name = "A1", X = 0.0, Y = 0.0, Z = 0.0 },
                    new UwbAnchor { Name = "A2", X = 6.0, Y = 0.0, Z = 0.0 },
                    new UwbAnchor { Name = "A3", X = 6.0, Y = 3.5, Z = 0.0 },
                    new UwbAnchor { Name = "A4", X = 0.0, Y = 3.5, Z = 0.0 }
                };

                await context.UwbAnchors.AddRangeAsync(anchors);
                await context.SaveChangesAsync();
                Console.WriteLine("UWB Anchors seeded successfully.");
            }
        }

        public static async Task SeedTestDataAsync(MottuContext context)
        {
            if (!await context.Motos.AnyAsync())
            {
                var motos = new[]
                {
                    new Moto { Chassi = "CHASSIS001", Placa = "ABC1234", Modelo = ModeloMoto.MottuSportESD, Status = MotoStatus.Disponivel, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Moto { Chassi = "CHASSIS002", Placa = "DEF5678", Modelo = ModeloMoto.MottuE, Status = MotoStatus.Disponivel, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Moto { Chassi = "CHASSIS003", Placa = "GHI9012", Modelo = ModeloMoto.MottuPop, Status = MotoStatus.Disponivel, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                await context.Motos.AddRangeAsync(motos);
                await context.SaveChangesAsync();
            }

            if (!await context.UwbTags.AnyAsync())
            {
                var tags = new[]
                {
                    new UwbTag { Eui64 = "tag01", Status = TagStatus.Ativa, MotoId = 1 },
                    new UwbTag { Eui64 = "tag02", Status = TagStatus.Ativa, MotoId = 2 },
                    new UwbTag { Eui64 = "tag03", Status = TagStatus.Ativa, MotoId = 3 }
                };

                await context.UwbTags.AddRangeAsync(tags);
                await context.SaveChangesAsync();
                Console.WriteLine("Test motos and tags seeded successfully.");
            }
        }
    }
}
