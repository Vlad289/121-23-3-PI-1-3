using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderDto orderDto)
        {
            try
            {
                var createdOrder = await _orderService.CreateOrderAsync(orderDto);
                return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{orderId:int}/items")]
        public async Task<IActionResult> AddItem(int orderId, [FromBody] OrderItemDto itemDto)
        {
            try
            {
                var updatedOrder = await _orderService.AddItemToOrderAsync(orderId, itemDto);
                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{orderId:int}/items/{itemId:int}")]
        public async Task<IActionResult> UpdateItemQuantity(int orderId, int itemId, [FromBody] int newQuantity)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderItemQuantityAsync(orderId, itemId, newQuantity);
                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{orderId:int}/items/{itemId:int}")]
        public async Task<IActionResult> RemoveItem(int orderId, int itemId)
        {
            try
            {
                await _orderService.RemoveItemFromOrderAsync(orderId, itemId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
