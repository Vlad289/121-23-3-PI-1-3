using AutoMapper;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Interfaces;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
        {
            if (string.IsNullOrWhiteSpace(productDto.Name))
                throw new ValidationException("Product name cannot be empty");

            if (productDto.Price <= 0)
                throw new ValidationException("Product price must be greater than zero");

            if (productDto.Quantity < 0)
                throw new ValidationException("Product quantity cannot be negative");

            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Product", id);

            var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.ProductId == id);
            if (orderItems.Any())
                throw new InvalidOperationBusinessException($"Cannot delete product with ID {id} because it is referenced in orders");

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Product", id);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> IsProductInStockAsync(int id, int quantity)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Product", id);

            return product.Quantity >= quantity;
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllProductsAsync();

            var products = await _unitOfWork.Products.FindAsync(
                p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> UpdateProductAsync(ProductDto productDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productDto.Id)
                ?? throw new EntityNotFoundException("Product", productDto.Id);

            if (string.IsNullOrWhiteSpace(productDto.Name))
                throw new ValidationException("Product name cannot be empty");

            if (productDto.Price <= 0)
                throw new ValidationException("Product price must be greater than zero");

            if (productDto.Quantity < 0)
                throw new ValidationException("Product quantity cannot be negative");

            // Update properties
            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.Quantity = productDto.Quantity;
            product.Description = productDto.Description;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task UpdateStockAsync(int id, int quantityChange)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Product", id);

            int newQuantity = product.Quantity + quantityChange;

            if (newQuantity < 0)
                throw new InsufficientStockException(product.Name, Math.Abs(quantityChange), product.Quantity);

            product.Quantity = newQuantity;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}