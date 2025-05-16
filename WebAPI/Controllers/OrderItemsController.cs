using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemsController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _orderItemService.GetAllOrderItemsAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _orderItemService.GetOrderItemByIdAsync(id);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            try
            {
                var items = await _orderItemService.GetOrderItemsByOrderIdAsync(orderId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderItemDto orderItemDto)
        {
            try
            {
                var createdItem = await _orderItemService.CreateOrderItemAsync(orderItemDto);
                return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderItemDto orderItemDto)
        {
            if (id != orderItemDto.Id)
                return BadRequest("ID mismatch");

            try
            {
                var updatedItem = await _orderItemService.UpdateOrderItemAsync(orderItemDto);
                return Ok(updatedItem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _orderItemService.DeleteOrderItemAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
