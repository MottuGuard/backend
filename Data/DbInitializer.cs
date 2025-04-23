using Microsoft.AspNetCore.Identity;

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
    }
}
