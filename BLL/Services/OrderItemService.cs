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
    public class OrderItemService : IOrderItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;

        public OrderItemService(IUnitOfWork unitOfWork, IMapper mapper, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productService = productService;
        }

        public async Task<OrderItemDto> CreateOrderItemAsync(OrderItemDto orderItemDto)
        {
            // Validate order exists
            var order = await _unitOfWork.Orders.GetByIdAsync(orderItemDto.OrderId)
                ?? throw new EntityNotFoundException("Order", orderItemDto.OrderId);

            // Validate product exists and has enough stock
            var product = await _unitOfWork.Products.GetByIdAsync(orderItemDto.ProductId)
                ?? throw new EntityNotFoundException("Product", orderItemDto.ProductId);

            if (product.Quantity < orderItemDto.Quantity)
                throw new InsufficientStockException(product.Name, orderItemDto.Quantity, product.Quantity);

            // Create order item
            var orderItem = _mapper.Map<OrderItem>(orderItemDto);
            orderItem.Price = product.Price; // Use current product price

            await _unitOfWork.OrderItems.AddAsync(orderItem);

            // Update product stock
            await _productService.UpdateStockAsync(product.Id, -orderItemDto.Quantity);

            await _unitOfWork.SaveChangesAsync();

            return await GetOrderItemWithDetailsAsync(orderItem.Id);
        }

        public async Task DeleteOrderItemAsync(int id)
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("OrderItem", id);

            // Return product quantity to stock
            await _productService.UpdateStockAsync(orderItem.ProductId, orderItem.Quantity);

            _unitOfWork.OrderItems.Remove(orderItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderItemDto>> GetAllOrderItemsAsync()
        {
            var orderItems = await _unitOfWork.OrderItems.GetAllAsync();
            var result = new List<OrderItemDto>();

            foreach (var item in orderItems)
            {
                result.Add(await GetOrderItemWithDetailsAsync(item.Id));
            }

            return result;
        }

        public async Task<OrderItemDto> GetOrderItemByIdAsync(int id)
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("OrderItem", id);

            return await GetOrderItemWithDetailsAsync(id);
        }

        public async Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            // Verify order exists
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
                ?? throw new EntityNotFoundException("Order", orderId);

            var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == orderId);
            var result = new List<OrderItemDto>();

            foreach (var item in orderItems)
            {
                result.Add(await GetOrderItemWithDetailsAsync(item.Id));
            }

            return result;
        }

        public async Task<bool> IsOrderItemExistsAsync(int id)
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(id);
            return orderItem != null;
        }

        public async Task<OrderItemDto> UpdateOrderItemAsync(OrderItemDto orderItemDto)
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(orderItemDto.Id)
                ?? throw new EntityNotFoundException("OrderItem", orderItemDto.Id);

            // Handle quantity changes
            if (orderItemDto.Quantity != orderItem.Quantity)
            {
                int quantityDifference = orderItemDto.Quantity - orderItem.Quantity;

                // If increasing quantity, check if enough stock is available
                if (quantityDifference > 0)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId)
                        ?? throw new EntityNotFoundException("Product", orderItem.ProductId);

                    if (product.Quantity < quantityDifference)
                        throw new InsufficientStockException(product.Name, quantityDifference, product.Quantity);
                }

                // Update stock
                await _productService.UpdateStockAsync(orderItem.ProductId, -quantityDifference);
            }

            // Update properties
            orderItem.Quantity = orderItemDto.Quantity;
            // Note: We usually don't update Price after creation as it represents the price at the time of order

            await _unitOfWork.SaveChangesAsync();

            return await GetOrderItemWithDetailsAsync(orderItem.Id);
        }

        // Helper methods
        private async Task<OrderItemDto> GetOrderItemWithDetailsAsync(int id)
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("OrderItem", id);

            var itemDto = _mapper.Map<OrderItemDto>(orderItem);

            // Get product name
            var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId);
            if (product != null)
            {
                itemDto.ProductName = product.Name;
            }

            return itemDto;
        }
    }
}