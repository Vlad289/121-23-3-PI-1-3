using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItemDto>> GetAllOrderItemsAsync();
        Task<OrderItemDto> GetOrderItemByIdAsync(int id);
        Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderIdAsync(int orderId);
        Task<OrderItemDto> CreateOrderItemAsync(OrderItemDto orderItemDto);
        Task<OrderItemDto> UpdateOrderItemAsync(OrderItemDto orderItemDto);
        Task DeleteOrderItemAsync(int id);
        Task<bool> IsOrderItemExistsAsync(int id);
    }
}