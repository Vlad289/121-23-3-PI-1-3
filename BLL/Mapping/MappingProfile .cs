using AutoMapper;
using BLL.DTOs;
using DAL.Models;
using System.Linq;

namespace BLL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product Mappings
            CreateMap<Product, ProductDto>().ReverseMap();

            // User Mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            CreateMap<UserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            // Order Mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<OrderDto, Order>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore());

            // OrderItem Mappings
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

            CreateMap<OrderItemDto, OrderItem>()
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());
        }
    }
}
