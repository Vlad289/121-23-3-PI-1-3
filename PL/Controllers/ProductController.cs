using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;

namespace PL.Controllers
{
    /// <summary>
    /// Controller for handling product operations
    /// </summary>
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;

        public ProductController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _productService = GetService<IProductService>();
        }

        /// <summary>
        /// Get all products
        /// </summary>
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            return await _productService.GetAllProductsAsync();
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            try
            {
                return await _productService.GetProductByIdAsync(id);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Search products by name or description
        /// </summary>
        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            return await _productService.SearchProductsAsync(searchTerm);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
        {
            try
            {
                return await _productService.CreateProductAsync(productDto);
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        public async Task<ProductDto> UpdateProductAsync(ProductDto productDto)
        {
            try
            {
                return await _productService.UpdateProductAsync(productDto);
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
        }

        /// <summary>
        /// Delete a product by ID
        /// </summary>
        public async Task DeleteProductAsync(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            catch (InvalidOperationBusinessException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if a product has enough stock
        /// </summary>
        public async Task<bool> CheckStockAsync(int id, int quantity)
        {
            try
            {
                return await _productService.IsProductInStockAsync(id, quantity);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update product stock
        /// </summary>
        public async Task UpdateStockAsync(int id, int quantityChange)
        {
            try
            {
                await _productService.UpdateStockAsync(id, quantityChange);
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
    }
}