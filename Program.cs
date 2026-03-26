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
            builder.Services.AddHttpClient<DinoApiClient>();
            builder.Services.AddScoped<DinoApiClient>();

            var app = builder.Build();

            // Настройка pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // ВАЖНО: статические файлы (изображения будут здесь)
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