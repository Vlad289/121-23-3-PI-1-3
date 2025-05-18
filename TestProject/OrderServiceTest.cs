using AutoMapper;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using BLL.Services;
using DAL.Interfaces;
using DAL.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _productServiceMock = new Mock<IProductService>();
        _orderService = new OrderService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _productServiceMock.Object);
    }


    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new List<Order> { new Order { Id = 1, UserId = 1 } };

        _unitOfWorkMock.Setup(u => u.Orders.GetAllAsync())
            .ReturnsAsync(orders);

        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(new OrderDto());

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User());

        _unitOfWorkMock.Setup(u => u.OrderItems.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<OrderItem, bool>>>()))
            .ReturnsAsync(new List<OrderItem>());

        // Act
        var result = await _orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteOrderAsync_ShouldDeleteOrderAndRestoreStock()
    {
        // Arrange
        var order = new Order { Id = 1, UserId = 1 };
        var orderItems = new List<OrderItem>
        {
            new OrderItem { ProductId = 1, Quantity = 2 }
        };

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(u => u.OrderItems.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<OrderItem, bool>>>()))
            .ReturnsAsync(orderItems);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _orderService.DeleteOrderAsync(order.Id);

        // Assert
        _unitOfWorkMock.Verify(u => u.OrderItems.Remove(It.IsAny<OrderItem>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Orders.Remove(order), Times.Once);
        _productServiceMock.Verify(p => p.UpdateStockAsync(1, 2), Times.Once);
    }

    [Fact]
    public async Task GetUserOrdersAsync_ShouldReturnUserOrders()
    {
        // Arrange
        var userId = 1;
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(new User());

        _unitOfWorkMock.Setup(u => u.Orders.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .ReturnsAsync(new List<Order> { new Order { Id = 1, UserId = userId } });

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User());

        _unitOfWorkMock.Setup(u => u.OrderItems.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<OrderItem, bool>>>()))
            .ReturnsAsync(new List<OrderItem>());

        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(new OrderDto());

        // Act
        var result = await _orderService.GetUserOrdersAsync(userId);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RemoveItemFromOrderAsync_ShouldRemoveItemAndRestoreStock()
    {
        // Arrange
        var orderId = 1;
        var itemId = 2;

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(orderId))
            .ReturnsAsync(new Order { Id = orderId });

        _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(itemId))
            .ReturnsAsync(new OrderItem { Id = itemId, OrderId = orderId, ProductId = 1, Quantity = 5 });

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _orderService.RemoveItemFromOrderAsync(orderId, itemId);

        // Assert
        _unitOfWorkMock.Verify(u => u.OrderItems.Remove(It.IsAny<OrderItem>()), Times.Once);
        _productServiceMock.Verify(p => p.UpdateStockAsync(1, 5), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderItemQuantityAsync_ShouldUpdateQuantityAndStock()
    {
        // Arrange
        var orderId = 1;
        var itemId = 2;
        var newQuantity = 10;

        var orderItem = new OrderItem { Id = itemId, OrderId = orderId, ProductId = 1, Quantity = 5 };
        var product = new Product { Id = 1, Quantity = 10 };

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(orderId))
            .ReturnsAsync(new Order { Id = orderId });

        _unitOfWorkMock.Setup(u => u.OrderItems.GetByIdAsync(itemId))
            .ReturnsAsync(orderItem);

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(new OrderDto());

        _unitOfWorkMock.Setup(u => u.OrderItems.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<OrderItem, bool>>>()))
            .ReturnsAsync(new List<OrderItem>());

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User());

        // Act
        var result = await _orderService.UpdateOrderItemQuantityAsync(orderId, itemId, newQuantity);

        // Assert
        Assert.NotNull(result);
        _productServiceMock.Verify(p => p.UpdateStockAsync(1, -5), Times.Once);
    }
}
