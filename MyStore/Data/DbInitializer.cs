using Microsoft.AspNetCore.Identity;
using MyStore.Models;

namespace MyStore.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // --- قائمة الأدوار التي يجب أن تكون موجودة في النظام ---
            string[] roleNames = { "Admin", "StoreManager" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // أنشئ الدور إذا لم يكن موجودًا
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // --- إنشاء المستخدم المدير الافتراضي ---
            var adminUser = await userManager.FindByEmailAsync("admin@mystore.com");
            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser
                {
                    UserName = "admin@mystore.com",
                    Email = "admin@mystore.com",
                    FullName = "Admin User",
                    EmailConfirmed = true // تأكيد البريد الإلكتروني تلقائيًا
                };
                var result = await userManager.CreateAsync(newAdminUser, "Admin_12345");
                if (result.Succeeded)
                {
                    // قم بتعيين دور "Admin" للمستخدم الجديد
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
            }
        }
    }
}

