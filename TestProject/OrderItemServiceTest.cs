using AutoMapper;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using BLL.Services;
using DAL.Interfaces;
using DAL.Models;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class OrderItemServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly OrderItemService _service;

    public OrderItemServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _productServiceMock = new Mock<IProductService>();
        _service = new OrderItemService(_unitOfWorkMock.Object, _mapperMock.Object, _productServiceMock.Object);
    }

    [Fact]
    public async Task CreateOrderItemAsync_Success()
    {
        var orderItemDto = new OrderItemDto { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2 };
        var order = new Order { Id = 1 };
        var product = new Product { Id = 1, Quantity = 10, Price = 50, Name = "Test Product" };
        var orderItem = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, Price = 50 };

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(orderItemDto.OrderId)).ReturnsAsync(order);
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(orderItemDto.ProductId)).ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<OrderItem>(orderItemDto)).Returns(orderItem);
        _unitOfWorkMock.Setup(u => u.OrderItems.AddAsync(It.IsAny<OrderItem>())).Returns(Task.CompletedTask);
        _productServiceMock.Setup(p => p.UpdateStockAsync(product.Id, -orderItemDto.Quantity)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(orderItem.Id)).ReturnsAsync(orderItem);
        _mapperMock.Setup(m => m.Map<OrderItemDto>(orderItem)).Returns(new OrderItemDto
        {
            Id = orderItem.Id,
            OrderId = orderItem.OrderId,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            ProductName = product.Name
        });
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(orderItem.ProductId)).ReturnsAsync(product);

        var result = await _service.CreateOrderItemAsync(orderItemDto);

        Assert.NotNull(result);
        Assert.Equal(orderItemDto.Quantity, result.Quantity);
        Assert.Equal(product.Name, result.ProductName);
    }

    [Fact]
    public async Task DeleteOrderItemAsync_Success()
    {
        var orderItem = new OrderItem { Id = 1, ProductId = 1, Quantity = 3 };

        _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(orderItem.Id)).ReturnsAsync(orderItem);
        _productServiceMock.Setup(p => p.UpdateStockAsync(orderItem.ProductId, orderItem.Quantity)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.OrderItems.Remove(orderItem));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _service.DeleteOrderItemAsync(orderItem.Id);

        _unitOfWorkMock.Verify(u => u.OrderItems.Remove(orderItem), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllOrderItemsAsync_ReturnsItems()
    {
        var orderItems = new List<OrderItem>
        {
            new OrderItem { Id = 1, ProductId = 1, Quantity = 2 },
            new OrderItem { Id = 2, ProductId = 2, Quantity = 5 }
        };

        _unitOfWorkMock.Setup(u => u.OrderItems.GetAllAsync()).ReturnsAsync(orderItems);

        foreach (var item in orderItems)
        {
            _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(item.Id)).ReturnsAsync(item);
            _mapperMock.Setup(m => m.Map<OrderItemDto>(item)).Returns(new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            });
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(item.ProductId)).ReturnsAsync(new Product { Name = $"Product {item.ProductId}" });
        }

        var result = await _service.GetAllOrderItemsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, ((List<OrderItemDto>)result).Count);
    }

    [Fact]
    public async Task GetOrderItemByIdAsync_ReturnsItem()
    {
        var orderItem = new OrderItem { Id = 1, ProductId = 1, Quantity = 4 };
        var product = new Product { Id = 1, Name = "Prod1" };

        _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(orderItem.Id)).ReturnsAsync(orderItem);
        _mapperMock.Setup(m => m.Map<OrderItemDto>(orderItem)).Returns(new OrderItemDto
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            ProductName = product.Name
        });
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(orderItem.ProductId)).ReturnsAsync(product);

        var result = await _service.GetOrderItemByIdAsync(orderItem.Id);

        Assert.NotNull(result);
        Assert.Equal(orderItem.Id, result.Id);
        Assert.Equal(product.Name, result.ProductName);
    }

    [Fact]
    public async Task GetOrderItemsByOrderIdAsync_ReturnsItems()
    {
        int orderId = 1;
        var order = new Order { Id = orderId };
        var orderItems = new List<OrderItem>
        {
            new OrderItem { Id = 1, OrderId = orderId, ProductId = 1, Quantity = 3 }
        };

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(orderId)).ReturnsAsync(order);
        _unitOfWorkMock.Setup(u => u.OrderItems.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<OrderItem, bool>>>()))
            .ReturnsAsync(orderItems);

        foreach (var item in orderItems)
        {
            _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(item.Id)).ReturnsAsync(item);
            _mapperMock.Setup(m => m.Map<OrderItemDto>(item)).Returns(new OrderItemDto
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            });
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(item.ProductId)).ReturnsAsync(new Product { Name = $"Product {item.ProductId}" });
        }

        var result = await _service.GetOrderItemsByOrderIdAsync(orderId);

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task IsOrderItemExistsAsync_ReturnsTrue()
    {
        int id = 1;
        var orderItem = new OrderItem { Id = id };

        _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(id)).ReturnsAsync(orderItem);

        var result = await _service.IsOrderItemExistsAsync(id);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_Success()
    {
        var existingOrderItem = new OrderItem { Id = 1, ProductId = 1, Quantity = 2 };
        var updatedDto = new OrderItemDto { Id = 1, Quantity = 5 };

        var product = new Product { Id = 1, Quantity = 10 };

        _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(updatedDto.Id)).ReturnsAsync(existingOrderItem);
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(existingOrderItem.ProductId)).ReturnsAsync(product);
        _productServiceMock.Setup(p => p.UpdateStockAsync(existingOrderItem.ProductId, -(updatedDto.Quantity - existingOrderItem.Quantity)))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(existingOrderItem.Id)).ReturnsAsync(existingOrderItem);
        _mapperMock.Setup(m => m.Map<OrderItemDto>(existingOrderItem)).Returns(updatedDto);
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(existingOrderItem.ProductId)).ReturnsAsync(product);

        var result = await _service.UpdateOrderItemAsync(updatedDto);

        Assert.NotNull(result);
        Assert.Equal(updatedDto.Quantity, result.Quantity);
    }
}
