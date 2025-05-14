using BLL.DTOs;
using BLL.Exceptions;
using PL.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PL.UI
{
    /// <summary>
    /// UI handler for order operations
    /// </summary>
    public class OrderUI : BaseUI
    {
        private readonly OrderController _orderController;
        private readonly UserController _userController;
        private readonly ProductController _productController;

        public OrderUI(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _orderController = GetController<OrderController>();
            _userController = GetController<UserController>();
            _productController = GetController<ProductController>();
        }

        public override void ShowMenu()
        {
            bool exit = false;

            while (!exit)
            {
                DisplayHeader("Order Management");
                Console.WriteLine("1. View All Orders");
                Console.WriteLine("2. View Order Details");
                Console.WriteLine("3. View User Orders");
                Console.WriteLine("4. Create New Order");
                Console.WriteLine("5. Add Item to Order");
                Console.WriteLine("6. Update Order Item Quantity");
                Console.WriteLine("7. Remove Item from Order");
                Console.WriteLine("8. Delete Order");
                Console.WriteLine("0. Back to Main Menu");

                int choice = GetIntInput("\nEnter your choice: ", 0, 8);

                switch (choice)
                {
                    case 1:
                        ViewAllOrdersAsync().Wait();
                        break;
                    case 2:
                        ViewOrderDetailsAsync().Wait();
                        break;
                    case 3:
                        ViewUserOrdersAsync().Wait();
                        break;
                    case 4:
                        CreateOrderAsync().Wait();
                        break;
                    case 5:
                        AddItemToOrderAsync().Wait();
                        break;
                    case 6:
                        UpdateOrderItemQuantityAsync().Wait();
                        break;
                    case 7:
                        RemoveItemFromOrderAsync().Wait();
                        break;
                    case 8:
                        DeleteOrderAsync().Wait();
                        break;
                    case 0:
                        exit = true;
                        break;
                }
            }
        }

        private async Task ViewAllOrdersAsync()
        {
            DisplayHeader("All Orders");

            try
            {
                var orders = await _orderController.GetAllOrdersAsync();
                DisplayOrders(orders);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task ViewOrderDetailsAsync()
        {
            DisplayHeader("Order Details");

            int orderId = GetIntInput("Enter Order ID: ", 1);

            try
            {
                var order = await _orderController.GetOrderByIdAsync(orderId);
                DisplayOrderDetails(order);
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"Order with ID {orderId} not found.");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task ViewUserOrdersAsync()
        {
            DisplayHeader("User Orders");

            int userId = GetIntInput("Enter User ID: ", 1);

            try
            {
                // Verify user exists
                await _userController.GetUserByIdAsync(userId);

                var orders = await _orderController.GetUserOrdersAsync(userId);
                DisplayOrders(orders);
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"User with ID {userId} not found.");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task CreateOrderAsync()
        {
            DisplayHeader("Create New Order");

            try
            {
                // Display available users
                var users = await _userController.GetAllUsersAsync();
                Console.WriteLine("Available Users:");
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.Id}, Username: {user.Username}, Role: {user.Role}");
                }

                int userId = GetIntInput("\nEnter User ID: ", 1);

                // Verify user exists
                await _userController.GetUserByIdAsync(userId);

                // Create order
                var orderDto = new OrderDto
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                // Ask if user wants to add items now
                if (GetConfirmation("Do you want to add items to this order now?"))
                {
                    await AddItemsToNewOrder(orderDto);
                }
                else
                {
                    // Create empty order
                    var createdOrder = await _orderController.CreateOrderAsync(orderDto);
                    DisplaySuccess($"Order created with ID: {createdOrder.Id}");
                }
            }
            catch (EntityNotFoundException ex)
            {
                DisplayError(ex.Message);
            }
            catch (InsufficientStockException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task AddItemsToNewOrder(OrderDto orderDto)
        {
            bool addMoreItems = true;

            while (addMoreItems)
            {
                // Display available products
                var products = await _productController.GetAllProductsAsync();
                Console.WriteLine("\nAvailable Products:");
                foreach (var productItem in products)
                {
                    Console.WriteLine($"ID: {productItem.Id}, Name: {productItem.Name}, Price: ${productItem.Price}, In Stock: {productItem.Quantity}");
                }

                int productId = GetIntInput("\nEnter Product ID: ", 1);

                // Verify product exists and get details
                ProductDto selectedProduct;
                try
                {
                    selectedProduct = await _productController.GetProductByIdAsync(productId);
                }
                catch (EntityNotFoundException)
                {
                    DisplayError($"Product with ID {productId} not found.");
                    continue;
                }

                int quantity = GetIntInput($"Enter quantity (max {selectedProduct.Quantity}): ", 1, selectedProduct.Quantity);

                // Add item to order
                orderDto.Items.Add(new OrderItemDto
                {
                    ProductId = productId,
                    ProductName = selectedProduct.Name,
                    Quantity = quantity,
                    Price = selectedProduct.Price
                });

                addMoreItems = GetConfirmation("Do you want to add more items?");
            }

            // Calculate total amount
            orderDto.CalculateTotalAmount();

            // Create order with items
            var createdOrder = await _orderController.CreateOrderAsync(orderDto);
            DisplayOrderDetails(createdOrder);
            DisplaySuccess($"Order created with ID: {createdOrder.Id}");
        }

        private async Task AddItemToOrderAsync()
        {
            DisplayHeader("Add Item to Order");

            int orderId = GetIntInput("Enter Order ID: ", 1);

            try
            {
                // Verify order exists
                var order = await _orderController.GetOrderByIdAsync(orderId);

                // Display available products
                var products = await _productController.GetAllProductsAsync();
                Console.WriteLine("\nAvailable Products:");
                foreach (var productItem in products)
                {
                    Console.WriteLine($"ID: {productItem.Id}, Name: {productItem.Name}, Price: ${productItem.Price}, In Stock: {productItem.Quantity}");
                }

                int productId = GetIntInput("\nEnter Product ID: ", 1);

                // Verify product exists and get details
                ProductDto selectedProduct;
                try
                {
                    selectedProduct = await _productController.GetProductByIdAsync(productId);
                }
                catch (EntityNotFoundException)
                {
                    DisplayError($"Product with ID {productId} not found.");
                    return;
                }

                int quantity = GetIntInput($"Enter quantity (max {selectedProduct.Quantity}): ", 1, selectedProduct.Quantity);

                // Create order item
                var itemDto = new OrderItemDto
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = selectedProduct.Price
                };

                // Add item to order
                var updatedOrder = await _orderController.AddItemToOrderAsync(orderId, itemDto);
                DisplayOrderDetails(updatedOrder);
                DisplaySuccess("Item added to order successfully.");
            }
            catch (EntityNotFoundException ex)
            {
                DisplayError(ex.Message);
            }
            catch (InsufficientStockException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }
        private async Task UpdateOrderItemQuantityAsync()
        {
            DisplayHeader("Update Order Item Quantity");

            int orderId = GetIntInput("Enter Order ID: ", 1);

            try
            {
                // Verify order exists and display items
                var order = await _orderController.GetOrderByIdAsync(orderId);
                Console.WriteLine("\nOrder Items:");
                foreach (var item in order.Items)
                {
                    Console.WriteLine($"Item ID: {item.Id}, Product: {item.ProductName}, Quantity: {item.Quantity}, Price: ${item.Price}");
                }

                int itemId = GetIntInput("\nEnter Item ID: ", 1);
                int newQuantity = GetIntInput("Enter new quantity: ", 1);

                // Update item quantity
                var updatedOrder = await _orderController.UpdateOrderItemQuantityAsync(orderId, itemId, newQuantity);
                DisplayOrderDetails(updatedOrder);
                DisplaySuccess("Item quantity updated successfully.");
            }
            catch (EntityNotFoundException ex)
            {
                DisplayError(ex.Message);
            }
            catch (ValidationException ex)
            {
                DisplayError(ex.Message);
            }
            catch (InsufficientStockException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task RemoveItemFromOrderAsync()
        {
            DisplayHeader("Remove Item from Order");

            int orderId = GetIntInput("Enter Order ID: ", 1);

            try
            {
                // Verify order exists and display items
                var order = await _orderController.GetOrderByIdAsync(orderId);
                Console.WriteLine("\nOrder Items:");
                foreach (var item in order.Items)
                {
                    Console.WriteLine($"Item ID: {item.Id}, Product: {item.ProductName}, Quantity: {item.Quantity}, Price: ${item.Price}");
                }

                int itemId = GetIntInput("\nEnter Item ID to remove: ", 1);

                // Remove item from order
                await _orderController.RemoveItemFromOrderAsync(orderId, itemId);
                DisplaySuccess("Item removed from order successfully.");
            }
            catch (EntityNotFoundException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task DeleteOrderAsync()
        {
            DisplayHeader("Delete Order");

            int orderId = GetIntInput("Enter Order ID: ", 1);

            try
            {
                // Verify order exists
                var order = await _orderController.GetOrderByIdAsync(orderId);
                DisplayOrderDetails(order);

                if (GetConfirmation("\nAre you sure you want to delete this order?"))
                {
                    await _orderController.DeleteOrderAsync(orderId);
                    DisplaySuccess("Order deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Operation cancelled.");
                }
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"Order with ID {orderId} not found.");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private void DisplayOrders(IEnumerable<OrderDto> orders)
        {
            Console.WriteLine("\nOrders List:");
            Console.WriteLine($"{"ID",-5} {"User",-15} {"Date",-20} {"Items",-10} {"Total Amount",-15}");
            Console.WriteLine(new string('-', 65));

            foreach (var order in orders)
            {
                Console.WriteLine($"{order.Id,-5} {order.Username,-15} {order.CreatedAt.ToString("yyyy-MM-dd HH:mm"),-20} {order.Items.Count,-10} ${order.TotalAmount,-15:F2}");
            }
        }

        private void DisplayOrderDetails(OrderDto order)
        {
            Console.WriteLine($"\nOrder ID: {order.Id}");
            Console.WriteLine($"User: {order.Username} (ID: {order.UserId})");
            Console.WriteLine($"Date: {order.CreatedAt.ToString("yyyy-MM-dd HH:mm")}");

            Console.WriteLine("\nItems:");
            Console.WriteLine($"{"ID",-5} {"Product",-30} {"Quantity",-10} {"Price",-10} {"Total",-10}");
            Console.WriteLine(new string('-', 65));

            foreach (var item in order.Items)
            {
                Console.WriteLine($"{item.Id,-5} {item.ProductName,-30} {item.Quantity,-10} ${item.Price,-10:F2} ${item.TotalPrice,-10:F2}");
            }

            Console.WriteLine(new string('-', 65));
            Console.WriteLine($"Total Order Amount: ${order.TotalAmount:F2}");
        }
    }
}