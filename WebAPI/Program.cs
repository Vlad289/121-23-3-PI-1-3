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

            // ��������� ��������� ���� �����
            builder.Services.AddDbContext<OnlineShopDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ��������� ���������� � UnitOfWork
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // ���� ��������� UnitOfWork

            // ��������� AutoMapper � �������� ������
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // ��������� ������ �����-�����
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IOrderItemService, OrderItemService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderItemService, OrderItemService>();

            // ������ ����������
            builder.Services.AddControllers();

            // ������ �������� Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ������������ ���������� ��������
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // ���� ������ ����������� - Middleware ��� �����������
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
