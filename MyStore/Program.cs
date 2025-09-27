using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Enable Roles
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages(options =>
{
    // Secure the admin area
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

// Add Authorization Policy for Admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Ensure Authentication is enabled
app.UseAuthorization();

app.MapRazorPages();


// === SEED DATABASE SECTION ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Call the method to seed roles, admin user, companies, and products
        await SeedData(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// === END OF SEED DATABASE SECTION ===


app.Run();


// === DATA SEEDING METHOD ===
static async Task SeedData(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    // 1. Seed Roles and Admin User (No changes here)
    string adminRoleName = "Admin";
    string adminEmail = "admin@admin.com";
    string adminPassword = "Oe@123456";

    if (!await roleManager.RoleExistsAsync(adminRoleName))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRoleName));
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Admin User", EmailConfirmed = true };
        await userManager.CreateAsync(adminUser, adminPassword);
        await userManager.AddToRoleAsync(adminUser, adminRoleName);
    }

    //// 2. Seed Companies
    //if (!context.Companies.Any())
    //{
    //    var companies = new List<Company>
    //    {
    //        new Company { Name = "المتجر العصري" }, new Company { Name = "البيت الأنيق" }, new Company { Name = "تقنية المستقبل" },
    //        new Company { Name = "الجودة العالية" }, new Company { Name = "النسيج الذهبي" }, new Company { Name = "عالم الإلكترونيات" },
    //        new Company { Name = "الأصالة للتجارة" }, new Company { Name = "الحلول الذكية" }, new Company { Name = "الشرق للتوريدات" },
    //        new Company { Name = "قمة الإبداع" }, new Company { Name = "النور للتوزيع" }, new Company { Name = "واحة الجمال" },
    //        new Company { Name = "ركن الأطفال" }, new Company { Name = "الفخامة للعطور" }, new Company { Name = "المذاق الأصيل" },
    //        new Company { Name = "الإخوة المتحدون" }, new Company { Name = "بوابة التجارة" }, new Company { Name = "إبداعات الورق" },
    //        new Company { Name = "روائع الخشب" }, new Company { Name = "زهور الربيع" }, new Company { Name = "عالم الرياضة" },
    //        new Company { Name = "الأمانة للتجارة" }, new Company { Name = "الإتقان للصناعة" }, new Company { Name = "كنوز الطبيعة" },
    //        new Company { Name = "المستقبل الرقمي" }, new Company { Name = "النجمة الساطعة" }, new Company { Name = "الريادة للتسويق" },
    //        new Company { Name = "بيت الهدايا" }, new Company { Name = "لؤلؤة الخليج" }, new Company { Name = "الفن الراقي" },
    //        new Company { Name = "إكسسوارات العصر" }, new Company { Name = "نكهات شرقية" }, new Company { Name = "الضوء الساطع" },
    //        new Company { Name = "أبراج المدينة" }, new Company { Name = "الأفق الواسع" }, new Company { Name = "ينابيع الخير" },
    //        new Company { Name = "السحاب الأبيض" }, new Company { Name = "عالم الموضة" }, new Company { Name = "الأيدي الماهرة" },
    //        new Company { Name = "عطور الياسمين" }, new Company { Name = "الجسر الذهبي" }, new Company { Name = "الرواد للتكنولوجيا" },
    //        new Company { Name = "أرض الزعفران" }, new Company { Name = "مملكة الحلويات" }, new Company { Name = "الفارس الأصيل" },
    //        new Company { Name = "شمس الأصيل" }, new Company { Name = "قمر الزمان" }, new Company { Name = "درة الشرق" },
    //        new Company { Name = "القمة للتجارة" }, new Company { Name = "بساتين الشام" }
    //    };
    //    await context.Companies.AddRangeAsync(companies);
    //    await context.SaveChangesAsync();
    //}

    //// 3. Seed Products for each company
    //if (!context.Products.Any())
    //{
    //    var allCompanies = await context.Companies.ToListAsync();
    //    var productsToCreate = new List<Product>();
    //    var random = new Random();

    //    // Lists for generating random product names
    //    var prefixes = new[] { "مجموعة", "جهاز", "عطر", "كريم", "قميص", "بنطلون", "حذاء", "ساعة", "هاتف", "شاشة", "كتاب", "لعبة", "زيت", "حقيبة" };
    //    var nouns = new[] { "الأناقة", "الجمال", "الذكاء", "القوة", "الراحة", "الفخامة", "التميز", "الإبداع", "السرعة", "النقاء", "السعادة", "الأحلام" };
    //    var suffixes = new[] { "العصري", "الكلاسيكي", "الرقمي", "الطبيعي", "الفاخر", "المميز", "الذهبي", "الفضي", "الجديد", "المطور" };

    //    foreach (var company in allCompanies)
    //    {
    //        for (int i = 1; i <= 100; i++)
    //        {
    //            var prefix = prefixes[random.Next(prefixes.Length)];
    //            var noun = nouns[random.Next(nouns.Length)];
    //            var suffix = suffixes[random.Next(suffixes.Length)];
    //            var productName = $"{prefix} {noun} {suffix} #{i}";
    //            var price = new decimal(random.Next(50, 5000) + random.NextDouble());

    //            productsToCreate.Add(new Product
    //            {
    //                Name = productName,
    //                Description = $"وصف تفصيلي للمنتج الرائع '{productName}'. مصنوع من أجود المواد لضمان أفضل تجربة للمستخدم.",
    //                Price = Math.Round(price, 2),
    //                CompanyId = company.Id,
    //                ImageUrl = $"https://placehold.co/600x400/007bff/white?text={Uri.EscapeDataString(prefix)}" // Using placeholder images
    //            });
    //        }
    //    }
    //    await context.Products.AddRangeAsync(productsToCreate);
    //    await context.SaveChangesAsync();
    //}
}

