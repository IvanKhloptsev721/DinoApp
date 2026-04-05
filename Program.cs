using DinoApp.Services;

namespace DinoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавляем MVC
            builder.Services.AddControllersWithViews();

            // Добавляем сессии
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "DinoMir.Session";
            });

            // Добавляем HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Регистрируем сервисы
            builder.Services.AddHttpClient<DinoApiClient>();
            builder.Services.AddScoped<DinoApiClient>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            var app = builder.Build();

            // Настройка pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession(); // ВАЖНО: до UseRouting
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dinosaurs}/{action=Index}/{id?}");

            app.Run();
        }
    }
}