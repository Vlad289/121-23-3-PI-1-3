using System;
using System.Collections.Generic;

namespace BLL.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}