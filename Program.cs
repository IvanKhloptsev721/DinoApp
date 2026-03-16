// Program.cs
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

            // Регистрируем сервисы
            builder.Services.AddSingleton<DinoApiClient>();
            builder.Services.AddScoped<FileService>(); // Scoped, так как зависит от IWebHostEnvironment

            var app = builder.Build();

            // Настройка pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Важно для статических файлов (изображения будут здесь)
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            // Маршрут по умолчанию на Dinosaurs
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dinosaurs}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
