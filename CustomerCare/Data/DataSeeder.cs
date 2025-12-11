using CustomerCare.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace CustomerCare.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { RoleNames.Admin, RoleNames.Employee, RoleNames.User };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@support.local";
            var adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
                }
                else
                {
                   
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
                {
                    await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
                }
            }
        }
    }
}
