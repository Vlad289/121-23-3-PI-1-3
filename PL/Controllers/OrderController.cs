using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;

namespace PL.Controllers
{
    /// <summary>
    /// Controller for handling order operations
    /// </summary>
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _orderService = GetService<IOrderService>();
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            return await _orderService.GetAllOrdersAsync();
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            try
            {
                return await _orderService.GetOrderByIdAsync(id);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get orders for a specific user
        /// </summary>
        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
        {
            try
            {
                return await _orderService.GetUserOrdersAsync(userId);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto)
        {
            try
            {
                return await _orderService.CreateOrderAsync(orderDto);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            catch (InsufficientStockException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Add item to an order
        /// </summary>
        public async Task<OrderDto> AddItemToOrderAsync(int orderId, OrderItemDto itemDto)
        {
            try
            {
                return await _orderService.AddItemToOrderAsync(orderId, itemDto);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            catch (InsufficientStockException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Remove item from an order
        /// </summary>
        public async Task RemoveItemFromOrderAsync(int orderId, int itemId)
        {
            try
            {
                await _orderService.RemoveItemFromOrderAsync(orderId, itemId);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update order item quantity
        /// </summary>
        public async Task<OrderDto> UpdateOrderItemQuantityAsync(int orderId, int itemId, int newQuantity)
        {
            try
            {
                return await _orderService.UpdateOrderItemQuantityAsync(orderId, itemId, newQuantity);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                throw;
            }
            catch (InsufficientStockException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete an order
        /// </summary>
        public async Task DeleteOrderAsync(int id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}