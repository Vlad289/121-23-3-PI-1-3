using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);
        Task<OrderDto> CreateOrderAsync(OrderDto orderDto);
        Task<OrderDto> AddItemToOrderAsync(int orderId, OrderItemDto itemDto);
        Task RemoveItemFromOrderAsync(int orderId, int itemId);
        Task<OrderDto> UpdateOrderItemQuantityAsync(int orderId, int itemId, int newQuantity);
        Task DeleteOrderAsync(int id);
    }
}