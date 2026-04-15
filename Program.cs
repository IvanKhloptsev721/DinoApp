using DinoApp.Services;
using DinoApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DinoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Name = "DinoMir.Session";
            });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpClient<DinoApiClient>((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var apiUrl = "http://85.198.68.116:5000";
                client.BaseAddress = new Uri(apiUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "DinoApp/1.0");
            });

            // ========== IDENTITY ==========
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=dinoapp.db"));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = false;
            });

            // Регистрация сервисов
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<UserImportService>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
                await next();
            });

            // ========== ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ ==========
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // Создание ролей
                string[] roles = { "Admin", "User", "Developer" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                        Console.WriteLine($"Created role: {role}");
                    }
                }

                // Создание администратора по умолчанию
                var adminEmail = "admin@dinoapp.com";
                var adminUser = userManager.FindByEmailAsync(adminEmail).Result;
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    var result = userManager.CreateAsync(adminUser, "Admin123!").Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                        Console.WriteLine("Created default admin user");
                    }
                }

                // Создание разработчика по умолчанию
                var devEmail = "developer@dinoapp.com";
                var devUser = userManager.FindByEmailAsync(devEmail).Result;
                if (devUser == null)
                {
                    devUser = new IdentityUser
                    {
                        UserName = "developer",
                        Email = devEmail,
                        EmailConfirmed = true
                    };
                    var result = userManager.CreateAsync(devUser, "Dev123!").Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(devUser, "Developer").Wait();
                        userManager.AddToRoleAsync(devUser, "Admin").Wait();
                        Console.WriteLine("Created default developer user");
                    }
                }

                // Импорт пользователей из JSON
                try
                {
                    var importService = scope.ServiceProvider.GetRequiredService<UserImportService>();
                    var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");
                    if (File.Exists(jsonPath))
                    {
                        importService.ImportUsersFromJsonAsync(jsonPath).Wait();
                        Console.WriteLine("Imported users from JSON");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing users: {ex.Message}");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "page",
                pattern: "page/{slug}",
                defaults: new { controller = "Pages", action = "ViewPage" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dinosaurs}/{action=Index}/{id?}");

            EnsureDirectoriesExist();

            app.Run();
        }

        private static void EnsureDirectoriesExist()
        {
            var directories = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "dinosaurs")
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Console.WriteLine($"Created directory: {dir}");
                }
            }
        }
    }
}