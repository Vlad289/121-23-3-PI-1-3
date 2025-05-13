using AutoMapper;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productService = productService;
        }

        public async Task<OrderDto> AddItemToOrderAsync(int orderId, OrderItemDto itemDto)
        {
            var order = await GetOrderEntityAsync(orderId);

            // Check if product exists and has enough stock
            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId)
                ?? throw new EntityNotFoundException("Product", itemDto.ProductId);

            if (product.Quantity < itemDto.Quantity)
                throw new InsufficientStockException(product.Name, itemDto.Quantity, product.Quantity);

            // Check if the order already contains this product
            var existingItem = (await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == orderId && oi.ProductId == itemDto.ProductId)).FirstOrDefault();

            if (existingItem != null)
            {
                // Update existing item quantity
                int additionalQuantity = itemDto.Quantity;
                existingItem.Quantity += additionalQuantity;

                // Make sure we still have enough stock
                if (product.Quantity < additionalQuantity)
                    throw new InsufficientStockException(product.Name, additionalQuantity, product.Quantity);

                // Update product stock
                await _productService.UpdateStockAsync(product.Id, -additionalQuantity);
            }
            else
            {
                // Create new order item
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    Price = product.Price // Use current product price
                };

                await _unitOfWork.OrderItems.AddAsync(orderItem);

                // Update product stock
                await _productService.UpdateStockAsync(product.Id, -itemDto.Quantity);
            }

            await _unitOfWork.SaveChangesAsync();

            // Return updated order
            return await GetOrderByIdAsync(orderId);
        }

        public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto)
        {
            // Verify user exists
            var user = await _unitOfWork.Users.GetByIdAsync(orderDto.UserId)
                ?? throw new EntityNotFoundException("User", orderDto.UserId);

            // Create order
            var order = new Order
            {
                UserId = orderDto.UserId,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Add items if any
            if (orderDto.Items.Any())
            {
                foreach (var itemDto in orderDto.Items)
                {
                    await AddItemToOrderAsync(order.Id, itemDto);
                }
            }

            return await GetOrderByIdAsync(order.Id);
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await GetOrderEntityAsync(id);

            // Get all order items
            var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id);

            // Return products to stock
            foreach (var item in orderItems)
            {
                await _productService.UpdateStockAsync(item.ProductId, item.Quantity);
                _unitOfWork.OrderItems.Remove(item);
            }

            _unitOfWork.Orders.Remove(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            var result = new List<OrderDto>();

            foreach (var order in orders)
            {
                var orderDto = await MapOrderToDto(order);
                result.Add(orderDto);
            }

            return result;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await GetOrderEntityAsync(id);
            return await MapOrderToDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
        {
            // Verify user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new EntityNotFoundException("User", userId);

            var orders = await _unitOfWork.Orders.FindAsync(o => o.UserId == userId);
            var result = new List<OrderDto>();

            foreach (var order in orders)
            {
                var orderDto = await MapOrderToDto(order);
                result.Add(orderDto);
            }

            return result;
        }

        public async Task RemoveItemFromOrderAsync(int orderId, int itemId)
        {
            // Verify order exists
            var order = await GetOrderEntityAsync(orderId);

            // Find the order item
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(itemId);

            if (orderItem == null || orderItem.OrderId != orderId)
                throw new EntityNotFoundException($"Order item with ID {itemId} not found in order {orderId}");

            // Return product quantity to stock
            await _productService.UpdateStockAsync(orderItem.ProductId, orderItem.Quantity);

            // Remove the item
            _unitOfWork.OrderItems.Remove(orderItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<OrderDto> UpdateOrderItemQuantityAsync(int orderId, int itemId, int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ValidationException("Quantity must be greater than zero");

            // Verify order exists
            var order = await GetOrderEntityAsync(orderId);

            // Find the order item
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(itemId);

            if (orderItem == null || orderItem.OrderId != orderId)
                throw new EntityNotFoundException($"Order item with ID {itemId} not found in order {orderId}");

            // Get the product to check stock
            var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId)
                ?? throw new EntityNotFoundException("Product", orderItem.ProductId);

            // Calculate stock change
            int quantityDifference = newQuantity - orderItem.Quantity;

            // If we need more items, check if enough stock is available
            if (quantityDifference > 0 && product.Quantity < quantityDifference)
                throw new InsufficientStockException(product.Name, quantityDifference, product.Quantity);

            // Update stock
            await _productService.UpdateStockAsync(product.Id, -quantityDifference);

            // Update order item quantity
            orderItem.Quantity = newQuantity;
            await _unitOfWork.SaveChangesAsync();

            // Return updated order
            return await GetOrderByIdAsync(orderId);
        }

        // Helper methods
        private async Task<Order> GetOrderEntityAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Order", id);

            return order;
        }

        private async Task<OrderDto> MapOrderToDto(Order order)
        {
            var orderDto = _mapper.Map<OrderDto>(order);

            // Get user info
            var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);
            if (user != null)
            {
                orderDto.Username = user.Username;
            }

            // Get order items
            var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == order.Id);
            var orderItemDtos = new List<OrderItemDto>();

            foreach (var item in orderItems)
            {
                var itemDto = _mapper.Map<OrderItemDto>(item);

                // Get product name
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    itemDto.ProductName = product.Name;
                }

                orderItemDtos.Add(itemDto);
            }

            orderDto.Items = orderItemDtos;

            return orderDto;
        }
    }
}