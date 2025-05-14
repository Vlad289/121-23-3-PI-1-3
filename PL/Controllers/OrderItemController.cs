using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;

namespace PL.Controllers
{
    /// <summary>
    /// Controller for handling order item operations
    /// </summary>
    public class OrderItemController : BaseController
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _orderItemService = GetService<IOrderItemService>();
        }

        /// <summary>
        /// Get all order items
        /// </summary>
        public async Task<IEnumerable<OrderItemDto>> GetAllOrderItemsAsync()
        {
            return await _orderItemService.GetAllOrderItemsAsync();
        }

        /// <summary>
        /// Get order item by ID
        /// </summary>
        public async Task<OrderItemDto> GetOrderItemByIdAsync(int id)
        {
            try
            {
                return await _orderItemService.GetOrderItemByIdAsync(id);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get order items by order ID
        /// </summary>
        public async Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            try
            {
                return await _orderItemService.GetOrderItemsByOrderIdAsync(orderId);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new order item
        /// </summary>
        public async Task<OrderItemDto> CreateOrderItemAsync(OrderItemDto orderItemDto)
        {
            try
            {
                return await _orderItemService.CreateOrderItemAsync(orderItemDto);
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
        /// Update an existing order item
        /// </summary>
        public async Task<OrderItemDto> UpdateOrderItemAsync(OrderItemDto orderItemDto)
        {
            try
            {
                return await _orderItemService.UpdateOrderItemAsync(orderItemDto);
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
        /// Delete an order item
        /// </summary>
        public async Task DeleteOrderItemAsync(int id)
        {
            try
            {
                await _orderItemService.DeleteOrderItemAsync(id);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if an order item exists
        /// </summary>
        public async Task<bool> IsOrderItemExistsAsync(int id)
        {
            return await _orderItemService.IsOrderItemExistsAsync(id);
        }
    }
}