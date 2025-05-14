using BLL.DTOs;
using BLL.Exceptions;
using PL.Controllers;

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

        /// <summary>
        /// Shows the order management menu
        /// </summary>
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
                Console.WriteLine("6. Remove Item from Order");
                Console.WriteLine("7. Update Order Item Quantity");
                Console.WriteLine("8. Delete Order");
                Console.WriteLine("0. Return to Main Menu");

                int choice = GetIntInput("\nEnter your choice: ", 0, 8);

                try
                {
                    switch (choice)
                    {
                        case 1:
                            await ViewAllOrdersAsync();
                            break;
                        case 2:
                            await ViewOrderDetailsAsync();
                            break;
                        case 3:
                            await ViewUserOrdersAsync();
                            break;
                        case 4:
                            await CreateOrderAsync();
                            break;
                        case 5:
                            await AddItemToOrderAsync();
                            break;
                        case 6:
                            await RemoveItemFromOrderAsync();
                            break;
                        case 7:
                            await UpdateOrderItemQuantityAsync();
                            break;
                        case 8:
                            await DeleteOrderAsync();
                            break;
                        case 0:
                            exit = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    DisplayError(ex.Message);
                    PressAnyKeyToContinue();
                }
            }
        }

        /// <summary>
        /// Displays all orders
        /// </summary>
        private async Task ViewAllOrdersAsync()
        {
            DisplayHeader("All Orders");

            var orders = await _orderController.GetAllOrdersAsync();

            if (!orders.Any())
            {
                Console.WriteLine("No orders found.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} {"Username",-20} {"Date",-20} {"Items",-10} {"Total",-10}");
                Console.WriteLine(new string('-', 65));

                foreach (var order in orders)
                {
                    Console.WriteLine($"{order.Id,-5} {order.Username,-20} {order.CreatedAt.ToString("yyyy-MM-dd HH:mm"),-20} {order.Items.Count,-10} {order.TotalAmount:C}");
                }
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Displays details of a specific order
        /// </summary>
        private async Task ViewOrderDetailsAsync()
        {
            DisplayHeader("Order Details");
            int orderId = GetIntInput("Enter Order ID: ");

            try
            {
                var order = await _orderController.GetOrderByIdAsync(orderId);
                DisplayOrderDetails(order);
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"Order with ID {orderId} not found.");
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Displays all orders for a specific user
        /// </summary>
        private async Task ViewUserOrdersAsync()
        {
            DisplayHeader("User Orders");

            // Get user first
            var users = await _userController.GetAllUsersAsync();
            if (!users.Any())
            {
                DisplayError("No users found.");
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine("Available Users:");
            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id}, Username: {user.Username}");
            }

            int userId = GetIntInput("\nEnter User ID: ");

            try
            {
                var orders = await _orderController.GetUserOrdersAsync(userId);

                if (!orders.Any())
                {
                    Console.WriteLine($"No orders found for user with ID {userId}.");
                }
                else
                {
                    var userName = orders.First().Username;
                    Console.WriteLine($"Orders for user: {userName} (ID: {userId})");
                    Console.WriteLine($"{"ID",-5} {"Date",-20} {"Items",-10} {"Total",-10}");
                    Console.WriteLine(new string('-', 45));

                    foreach (var order in orders)
                    {
                        Console.WriteLine($"{order.Id,-5} {order.CreatedAt.ToString("yyyy-MM-dd HH:mm"),-20} {order.Items.Count,-10} {order.TotalAmount:C}");
                    }
                }
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"User with ID {userId} not found.");
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Creates a new order
        /// </summary>
        private async Task CreateOrderAsync()
        {
            DisplayHeader("Create New Order");

            // Get users
            var users = await _userController.GetAllUsersAsync();
            if (!users.Any())
            {
                DisplayError("No users found. Please create a user first.");
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine("Available Users:");
            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id}, Username: {user.Username}");
            }

            int userId = GetIntInput("\nEnter User ID: ");

            try
            {
                // Check if user exists
                var user = await _userController.GetUserByIdAsync(userId);

                // Create a new order
                var orderDto = new OrderDto
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    Username = user.Username
                };

                // Ask if the user wants to add items to the order now
                if (GetConfirmation("Do you want to add items to the order now?"))
                {
                    await AddItemsToNewOrder(orderDto);
                }

                // Calculate total amount
                orderDto.CalculateTotalAmount();

                // Create the order
                var createdOrder = await _orderController.CreateOrderAsync(orderDto);
                DisplaySuccess($"Order created successfully with ID: {createdOrder.Id}");
                DisplayOrderDetails(createdOrder);
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"User with ID {userId} not found.");
            }
            catch (InsufficientStockException ex)
            {
                DisplayError($"Insufficient stock: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Adds items to a new order before creation
        /// </summary>
        private async Task AddItemsToNewOrder(OrderDto orderDto)
        {
            bool addMore = true;

            while (addMore)
            {
                // Get products
                var products = await _productController.GetAllProductsAsync();
                if (!products.Any())
                {
                    DisplayError("No products found. Please add products first.");
                    return;
                }

                Console.WriteLine("\nAvailable Products:");
                Console.WriteLine($"{"ID",-5} {"Name",-30} {"Price",-10} {"Stock",-10}");
                Console.WriteLine(new string('-', 55));

                foreach (var product in products)
                {
                    Console.WriteLine($"{product.Id,-5} {product.Name,-30} {product.Price:C,-10} {product.Quantity,-10}");
                }

                int productId = GetIntInput("\nEnter Product ID: ");

                try
                {
                    // Check if product exists
                    var product = await _productController.GetProductByIdAsync(productId);

                    int quantity = GetIntInput($"Enter Quantity (max {product.Quantity}): ", 1, product.Quantity);

                    // Check if product is already in the order
                    var existingItem = orderDto.Items.FirstOrDefault(i => i.ProductId == productId);

                    if (existingItem != null)
                    {
                        // Update existing item
                        existingItem.Quantity += quantity;
                        existingItem.CalculateTotalPrice();
                        DisplaySuccess($"Updated quantity of {product.Name} to {existingItem.Quantity}");
                    }
                    else
                    {
                        // Add new item
                        var orderItemDto = new OrderItemDto
                        {
                            ProductId = productId,
                            ProductName = product.Name,
                            Quantity = quantity,
                            Price = product.Price
                        };
                        orderItemDto.CalculateTotalPrice();
                        orderDto.Items.Add(orderItemDto);
                        DisplaySuccess($"Added {quantity} x {product.Name} to the order");
                    }

                    // Ask if user wants to add more items
                    addMore = GetConfirmation("Do you want to add more items?");
                }
                catch (EntityNotFoundException)
                {
                    DisplayError($"Product with ID {productId} not found.");
                    // Continue anyway
                    addMore = GetConfirmation("Do you want to try adding another item?");
                }
            }
        }

        /// <summary>
        /// Adds an item to an existing order
        /// </summary>
        private async Task AddItemToOrderAsync()
        {
            DisplayHeader("Add Item to Order");

            int orderId = GetIntInput("Enter Order ID: ");

            try
            {
                // Check if order exists
                var order = await _orderController.GetOrderByIdAsync(orderId);

                // Get products
                var products = await _productController.GetAllProductsAsync();
                if (!products.Any())
                {
                    DisplayError("No products found.");
                    PressAnyKeyToContinue();
                    return;
                }

                Console.WriteLine("\nAvailable Products:");
                Console.WriteLine($"{"ID",-5} {"Name",-30} {"Price",-10} {"Stock",-10}");
                Console.WriteLine(new string('-', 55));

                foreach (var product in products)
                {
                    Console.WriteLine($"{product.Id,-5} {product.Name,-30} {product.Price:C,-10} {product.Quantity,-10}");
                }

                int productId = GetIntInput("\nEnter Product ID: ");

                // Check if product exists
                var selectedProduct = await _productController.GetProductByIdAsync(productId);

                int quantity = GetIntInput($"Enter Quantity (max {selectedProduct.Quantity}): ", 1, selectedProduct.Quantity);

                // Check if the product is already in the order
                var existingItem = order.Items.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem != null)
                {
                    // Ask for confirmation to update existing item
                    if (GetConfirmation($"This product is already in the order with quantity {existingItem.Quantity}. Do you want to update it?"))
                    {
                        // Update via controller
                        await _orderController.UpdateOrderItemQuantityAsync(orderId, existingItem.Id, existingItem.Quantity + quantity);
                        DisplaySuccess($"Updated quantity of {selectedProduct.Name} in the order");
                    }
                }
                else
                {
                    // Create new order item
                    var orderItemDto = new OrderItemDto
                    {
                        OrderId = orderId,
                        ProductId = productId,
                        ProductName = selectedProduct.Name,
                        Quantity = quantity,
                        Price = selectedProduct.Price
                    };

                    // Add item to order
                    var updatedOrder = await _orderController.AddItemToOrderAsync(orderId, orderItemDto);
                    DisplaySuccess($"Added {quantity} x {selectedProduct.Name} to the order");
                    DisplayOrderDetails(updatedOrder);
                }
            }
            catch (EntityNotFoundException ex)
            {
                DisplayError(ex.Message);
            }
            catch (InsufficientStockException ex)
            {
                DisplayError($"Insufficient stock: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Removes an item from an order
        /// </summary>
        private async Task RemoveItemFromOrderAsync()
        {
            DisplayHeader("Remove Item from Order");

            int orderId = GetIntInput("Enter Order ID: ");

            try
            {
                // Check if order exists and display its items
                var order = await _orderController.GetOrderByIdAsync(orderId);

                if (!order.Items.Any())
                {
                    DisplayError("This order has no items.");
                    PressAnyKeyToContinue();
                    return;
                }

                DisplayOrderItems(order);

                int itemId = GetIntInput("\nEnter Item ID to remove: ");

                // Check if item exists in the order
                if (!order.Items.Any(i => i.Id == itemId))
                {
                    DisplayError($"Item with ID {itemId} not found in this order.");
                    PressAnyKeyToContinue();
                    return;
                }

                // Confirm deletion
                if (GetConfirmation("Are you sure you want to remove this item from the order?"))
                {
                    await _orderController.RemoveItemFromOrderAsync(orderId, itemId);
                    DisplaySuccess("Item removed from the order successfully");

                    // Show updated order
                    var updatedOrder = await _orderController.GetOrderByIdAsync(orderId);
                    DisplayOrderDetails(updatedOrder);
                }
            }
            catch (EntityNotFoundException ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Updates the quantity of an item in an order
        /// </summary>
        private async Task UpdateOrderItemQuantityAsync()
        {
            DisplayHeader("Update Order Item Quantity");

            int orderId = GetIntInput("Enter Order ID: ");

            try
            {
                // Check if order exists and display its items
                var order = await _orderController.GetOrderByIdAsync(orderId);

                if (!order.Items.Any())
                {
                    DisplayError("This order has no items.");
                    PressAnyKeyToContinue();
                    return;
                }

                DisplayOrderItems(order);

                int itemId = GetIntInput("\nEnter Item ID to update: ");

                // Check if item exists in the order
                var item = order.Items.FirstOrDefault(i => i.Id == itemId);
                if (item == null)
                {
                    DisplayError($"Item with ID {itemId} not found in this order.");
                    PressAnyKeyToContinue();
                    return;
                }

                // Get product information for stock check
                var product = await _productController.GetProductByIdAsync(item.ProductId);

                int newQuantity = GetIntInput($"Enter new quantity (max {product.Quantity}): ", 1, product.Quantity);

                var updatedOrder = await _orderController.UpdateOrderItemQuantityAsync(orderId, itemId, newQuantity);
                DisplaySuccess($"Item quantity updated successfully to {newQuantity}");
                DisplayOrderDetails(updatedOrder);
            }
            catch (EntityNotFoundException ex)
            {
                DisplayError(ex.Message);
            }
            catch (ValidationException ex)
            {
                DisplayError($"Validation error: {ex.Message}");
            }
            catch (InsufficientStockException ex)
            {
                DisplayError($"Insufficient stock: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        private async Task DeleteOrderAsync()
        {
            DisplayHeader("Delete Order");

            int orderId = GetIntInput("Enter Order ID: ");

            try
            {
                // Check if order exists
                var order = await _orderController.GetOrderByIdAsync(orderId);
                DisplayOrderDetails(order);

                // Confirm deletion
                if (GetConfirmation("\nAre you sure you want to delete this order?"))
                {
                    await _orderController.DeleteOrderAsync(orderId);
                    DisplaySuccess("Order deleted successfully");
                }
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"Order with ID {orderId} not found.");
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Helper method to display order details
        /// </summary>
        private void DisplayOrderDetails(OrderDto order)
        {
            Console.WriteLine($"\nOrder ID: {order.Id}");
            Console.WriteLine($"Customer: {order.Username} (ID: {order.UserId})");
            Console.WriteLine($"Created At: {order.CreatedAt}");

            if (order.Items.Any())
            {
                DisplayOrderItems(order);
                Console.WriteLine($"\nTotal Amount: {order.TotalAmount:C}");
            }
            else
            {
                Console.WriteLine("\nNo items in this order.");
            }
        }

        /// <summary>
        /// Helper method to display order items
        /// </summary>
        private void DisplayOrderItems(OrderDto order)
        {
            Console.WriteLine("\nOrder Items:");
            Console.WriteLine($"{"ID",-5} {"Product",-30} {"Price",-10} {"Quantity",-10} {"Total",-10}");
            Console.WriteLine(new string('-', 65));

            foreach (var item in order.Items)
            {
                Console.WriteLine($"{item.Id,-5} {item.ProductName,-30} {item.Price:C,-10} {item.Quantity,-10} {item.TotalPrice:C,-10}");
            }
        }
    }
}