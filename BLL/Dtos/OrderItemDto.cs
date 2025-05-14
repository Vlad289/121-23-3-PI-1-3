using System;
namespace BLL.DTOs
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!; // Additional field for convenient display
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; } // Змінено з обчислюваної властивості на звичайну

        // Метод для обчислення TotalPrice на основі Price та Quantity
        public void CalculateTotalPrice()
        {
            TotalPrice = Price * Quantity;
        }
    }
}