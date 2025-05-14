using AutoMapper;
using BLL.DTOs;
using DAL.Models;

namespace BLL.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Password, opt => opt.Ignore()); // Don't map PasswordHash to Password
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Handle password hashing in service
                .ForMember(dest => dest.Orders, opt => opt.Ignore()); // Don't map Orders collection

            // Product mappings
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();

            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Items, opt => opt.Ignore()) // Items handled separately
                .ForMember(dest => dest.Username, opt => opt.Ignore()) // Username handled separately
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()); // TotalAmount calculated separately
            CreateMap<OrderDto, Order>()
                .ForMember(dest => dest.Items, opt => opt.Ignore()) // Items handled separately
                .ForMember(dest => dest.User, opt => opt.Ignore()); // User handled separately

            // OrderItem mappings
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore()) // ProductName handled separately
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price * src.Quantity));
            CreateMap<OrderItemDto, OrderItem>()
                .ForMember(dest => dest.Order, opt => opt.Ignore()) // Order handled separately
                .ForMember(dest => dest.Product, opt => opt.Ignore()); // Product handled separately
        }
    }
}