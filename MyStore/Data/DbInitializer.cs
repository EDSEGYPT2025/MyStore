using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Manager"))
            {
                await roleManager.CreateAsync(new IdentityRole("Manager"));
            }

            // Seed Admin User
            var adminEmail = "admin@mystore.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin User",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        public static async Task SeedStoreDataAsync(ApplicationDbContext context)
        {
            var firstStore = await context.Stores.FirstOrDefaultAsync();
            if (firstStore == null) return;

            // === التحسين: إضافة العدد المتبقي من الماركات ===
            const int totalCompaniesToSeed = 10;
            var currentCompanyCount = await context.Companies.CountAsync(c => c.StoreId == firstStore.Id);

            if (currentCompanyCount < totalCompaniesToSeed)
            {
                var companiesToGenerate = totalCompaniesToSeed - currentCompanyCount;
                var companyFaker = new Faker<Company>()
                    .RuleFor(c => c.Name, f => f.Company.CompanyName())
                    .RuleFor(c => c.LogoUrl, f => f.Image.PicsumUrl(400, 400, true))
                    .RuleFor(c => c.StoreId, firstStore.Id);

                var newCompanies = companyFaker.Generate(companiesToGenerate);
                await context.Companies.AddRangeAsync(newCompanies);
                await context.SaveChangesAsync();
            }

            // === التحسين: إضافة العدد المتبقي من المنتجات ===
            const int totalProductsToSeed = 200;
            var currentProductCount = await context.Products.CountAsync(p => p.StoreId == firstStore.Id);

            // تأكد من وجود ماركات قبل إضافة المنتجات
            var storeCompanies = await context.Companies.Where(c => c.StoreId == firstStore.Id).ToListAsync();
            if (!storeCompanies.Any()) return; // لا تضف منتجات إذا لم تكن هناك ماركات

            if (currentProductCount < totalProductsToSeed)
            {
                var productsToGenerate = totalProductsToSeed - currentProductCount;
                var productFaker = new Faker<Product>("ar")
                    .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                    .RuleFor(p => p.Description, f => f.Lorem.Paragraphs(2))
                    .RuleFor(p => p.Price, f => Math.Round(f.Random.Decimal(50, 5000), 2))
                    .RuleFor(p => p.ImageUrl, f => f.Image.PicsumUrl(640, 480, true))
                    .RuleFor(p => p.StoreId, firstStore.Id)
                    .RuleFor(p => p.CompanyId, f => f.PickRandom(storeCompanies).Id);

                var newProducts = productFaker.Generate(productsToGenerate);
                await context.Products.AddRangeAsync(newProducts);
                await context.SaveChangesAsync();
            }
        }
    }
}