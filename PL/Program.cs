using BLL.Interfaces;
using BLL.Services;
using DAL;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PL.Controllers;
using PL.UI;
using System;
using AutoMapper;
using BLL.Mappings;

namespace PL
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure services
            var serviceProvider = ConfigureServices();

            // Start the application
            var mainMenu = new MainMenu(serviceProvider);
            mainMenu.Show();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register DbContext
            services.AddDbContext<OnlineShopDbContext>(options =>
                options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=OnlineShopDb;Trusted_Connection=True;TrustServerCertificate=True;"));

            // Register UnitOfWork instead of individual repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // Register services in the correct order (note ProductService needs to be registered before OrderItemService)
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<IOrderService, OrderService>();

            // Register controllers
            services.AddScoped<UserController>();
            services.AddScoped<ProductController>();
            services.AddScoped<OrderController>();
            services.AddScoped<OrderItemController>();

            // Register UI classes
            services.AddScoped<UserUI>();
            services.AddScoped<ProductUI>();
            services.AddScoped<OrderUI>();
            services.AddScoped<MainMenu>();

            return services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// Main menu for the application
    /// </summary>
    public class MainMenu : BaseUI
    {
        private readonly UserUI _userUI;
        private readonly ProductUI _productUI;
        private readonly OrderUI _orderUI;

        public MainMenu(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userUI = GetController<UserUI>();
            _productUI = GetController<ProductUI>();
            _orderUI = GetController<OrderUI>();
        }

        public void Show()
        {
            bool exit = false;

            // Display welcome message
            Console.Clear();
            Console.WriteLine("*********************************************");
            Console.WriteLine("*          ONLINE SHOP MANAGEMENT           *");
            Console.WriteLine("*********************************************");
            Console.WriteLine();
            PressAnyKeyToContinue();

            while (!exit)
            {
                DisplayHeader("Main Menu");
                Console.WriteLine("1. User Management");
                Console.WriteLine("2. Product Management");
                Console.WriteLine("3. Order Management");
                Console.WriteLine("0. Exit Application");

                int choice = GetIntInput("\nEnter your choice: ", 0, 3);

                switch (choice)
                {
                    case 1:
                        _userUI.ShowMenu();
                        break;
                    case 2:
                        _productUI.ShowMenu();
                        break;
                    case 3:
                        _orderUI.ShowMenu();
                        break;
                    case 0:
                        exit = true;
                        break;
                }
            }

            // Display goodbye message
            Console.Clear();
            Console.WriteLine("*********************************************");
            Console.WriteLine("*       Thank you for using our app!        *");
            Console.WriteLine("*********************************************");
            Console.WriteLine();
        }

        public override void ShowMenu()
        {
            Show();
        }
    }
}