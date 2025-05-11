using DAL;

namespace PL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 👉 ТУТ додаємо контекст БД
            builder.Services.AddDbContext<OnlineShopDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // інші сервіси
            builder.Services.AddControllers(); // якщо це API

            var app = builder.Build();

            // конфігурація маршрутизації
            app.UseAuthorization();
            app.MapControllers();

            app.Run();

            Console.WriteLine("Hello, World!");
        }
    }
}
