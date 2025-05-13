using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<ProductDto> CreateProductAsync(ProductDto productDto);
        Task<ProductDto> UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(int id);
        Task<bool> IsProductInStockAsync(int id, int quantity);
        Task UpdateStockAsync(int id, int quantityChange);
    }
}