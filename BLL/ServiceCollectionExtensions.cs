using BLL.Interfaces;
using BLL.Mappings;
using BLL.Services;
using DAL;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BLL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services, string connectionString)
        {
            // Register DbContext
            services.AddDbContext<OnlineShopDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register DAL services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register BLL services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}