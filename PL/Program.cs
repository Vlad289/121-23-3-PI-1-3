using BLL.Interfaces;
using BLL.Services;
using DAL;
using DAL.Context;
using DAL.Interfaces;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PL.Controllers;
using PL.UI;

namespace PL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            try
            {
                // Create and migrate database
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<OnlineShopDbContext>();
                    dbContext.Database.Migrate();
                    Console.WriteLine("Database connection successful.");
                }

                // Run the main menu
                var mainMenuUI = serviceProvider.GetRequiredService<MainMenuUI>();
                mainMenuUI.ShowMenu();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Application error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                Console.ReadKey();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Database
            services.AddDbContext<OnlineShopDbContext>(options =>
                options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=OnlineShopDb;Trusted_Connection=True;TrustServerCertificate=True;"));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();

            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderItemService, OrderItemService>();

            // Controllers
            services.AddScoped<UserController>();
            services.AddScoped<ProductController>();
            services.AddScoped<OrderController>();
            services.AddScoped<OrderItemController>();

            // UI
            services.AddScoped<MainMenuUI>();
            services.AddScoped<UserUI>();
            services.AddScoped<ProductUI>();
            services.AddScoped<OrderUI>();
            services.AddTransient<CurrentUserManager>();
        }
    }
}