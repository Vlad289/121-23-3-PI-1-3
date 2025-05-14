using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; } = null!; // Additional field for convenient display
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public decimal TotalAmount { get; set; } // Змінено з обчислюваної властивості на звичайну

        // Метод для обчислення загальної суми замовлення
        public void CalculateTotalAmount()
        {
            TotalAmount = Items.Sum(item => item.Price * item.Quantity);
        }
    }
}