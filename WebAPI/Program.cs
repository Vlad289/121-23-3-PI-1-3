using BLL.Interfaces;
using BLL.Mappings;
using BLL.Services;
using DAL;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Реєстрація контексту бази даних
            builder.Services.AddDbContext<OnlineShopDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Реєстрація репозиторіїв і UnitOfWork
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // Твоя реалізація UnitOfWork

            // Реєстрація AutoMapper з профілем мапінгу
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Реєстрація сервісів бізнес-логіки
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IOrderItemService, OrderItemService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderItemService, OrderItemService>();

            // Додати контролери
            builder.Services.AddControllers();

            // Додати підтримку Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Налаштування середовища розробки
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Якщо плануєш авторизацію - Middleware для авторизації
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
