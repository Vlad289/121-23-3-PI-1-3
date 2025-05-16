using AutoMapper;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services;
using DAL.Interfaces;
using DAL.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestProject
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepoMock;
        private readonly IMapper _mapper;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _productRepoMock = new Mock<IProductRepository>();
            _orderItemRepoMock = new Mock<IOrderItemRepository>();

            _unitOfWorkMock.SetupGet(u => u.Products).Returns(_productRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.OrderItems).Returns(_orderItemRepoMock.Object);

            var config = new MapperConfiguration(cfg => cfg.CreateMap<ProductDto, Product>().ReverseMap());
            _mapper = config.CreateMapper();

            _service = new ProductService(_unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task CreateProductAsync_ValidInput_ReturnsProductDto()
        {
            // Arrange
            var dto = new ProductDto { Name = "Test", Price = 10, Quantity = 5 };
            _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.CreateProductAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
        }

        [Fact]
        public async Task DeleteProductAsync_ProductNotFound_ThrowsException()
        {
            // Arrange
            _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteProductAsync(1));
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsProducts()
        {
            // Arrange
            var products = new List<Product> { new Product { Id = 1, Name = "P1" } };
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _service.GetAllProductsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("P1", result.First().Name);
        }

        [Fact]
        public async Task UpdateProductAsync_ProductNotFound_ThrowsException()
        {
            // Arrange
            var dto = new ProductDto { Id = 1, Name = "New" };
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateProductAsync(dto));
        }

        [Fact]
        public async Task UpdateStockAsync_InsufficientStock_ThrowsException()
        {
            // Arrange
            var product = new Product { Id = 1, Quantity = 3 };
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientStockException>(() => _service.UpdateStockAsync(1, -5));
        }
    }
}