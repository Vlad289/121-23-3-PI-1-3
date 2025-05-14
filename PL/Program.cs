using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PL
{
    internal class Program
    {
        private static IServiceProvider _serviceProvider;
        private static IUserService _userService;
        private static IProductService _productService;
        private static IOrderService _orderService;
        private static IOrderItemService _orderItemService;
        private static UserDto _currentUser;

        static async Task Main(string[] args)
        {
            SetupDependencyInjection();
            await InitializeServices();

            bool exit = false;
            Console.WriteLine("=== Online Shop Console Interface ===");

            // Try to login or continue as guest
            await LoginOrContinueAsGuest();

            while (!exit)
            {
                try
                {
                    ShowMainMenu();
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1": // Products menu
                            await ProductsMenuAsync();
                            break;
                        case "2": // Users menu
                            await UsersMenuAsync();
                            break;
                        case "3": // Orders menu
                            await OrdersMenuAsync();
                            break;
                        case "4": // Login/Logout
                            if (_currentUser != null && _currentUser.Role != "Unregistered")
                                Logout();
                            else
                                await LoginOrContinueAsGuest();
                            break;
                        case "0": // Exit
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }

            Console.WriteLine("Thank you for using Online Shop! Goodbye!");
        }

        private static void SetupDependencyInjection()
        {
            var services = new ServiceCollection();
            services.AddBusinessLogicServices("Server=localhost\\SQLEXPRESS;Database=OnlineShopDb;Trusted_Connection=True;TrustServerCertificate=True;");

            _serviceProvider = services.BuildServiceProvider();
        }

        private static async Task InitializeServices()
        {
            _userService = _serviceProvider.GetService<IUserService>();
            _productService = _serviceProvider.GetService<IProductService>();
            _orderService = _serviceProvider.GetService<IOrderService>();
            _orderItemService = _serviceProvider.GetService<IOrderItemService>();

            // Create default admin user if it doesn't exist
            try
            {
                await _userService.GetUserByUsernameAsync("admin");
            }
            catch
            {
                // Create admin user if it doesn't exist
                var adminUser = new UserDto
                {
                    Username = "admin",
                    Password = "admin123",
                    Role = "Admin"
                };
                await _userService.CreateUserAsync(adminUser);
                Console.WriteLine("Admin user created. Username: admin, Password: admin123");
            }
        }

        private static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Online Shop Main Menu ===");
            Console.WriteLine($"Logged in as: {(_currentUser != null ? $"{_currentUser.Username} ({_currentUser.Role})" : "Guest")}");
            Console.WriteLine();
            Console.WriteLine("1. Products");
            Console.WriteLine("2. Users");
            Console.WriteLine("3. Orders");
            Console.WriteLine("4. " + (_currentUser != null && _currentUser.Role != "Unregistered" ? "Logout" : "Login"));
            Console.WriteLine("0. Exit");
            Console.WriteLine();
            Console.Write("Enter your choice: ");
        }

        #region Authentication
        private static async Task LoginOrContinueAsGuest()
        {
            Console.Clear();
            Console.WriteLine("=== Login ===");
            Console.WriteLine("1. Login with existing account");
            Console.WriteLine("2. Register new account");
            Console.WriteLine("3. Continue as guest");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await LoginAsync();
                    break;
                case "2":
                    await RegisterAsync();
                    break;
                case "3":
                    _currentUser = new UserDto { Id = 0, Username = "Guest", Role = "Unregistered" };
                    Console.WriteLine("Continuing as guest...");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Continuing as guest...");
                    _currentUser = new UserDto { Id = 0, Username = "Guest", Role = "Unregistered" };
                    break;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task LoginAsync()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            bool isValid = await _userService.ValidateUserAsync(username, password);
            if (isValid)
            {
                _currentUser = await _userService.GetUserByUsernameAsync(username);
                Console.WriteLine($"Welcome, {_currentUser.Username}!");
            }
            else
            {
                Console.WriteLine("Invalid username or password.");
                _currentUser = new UserDto { Id = 0, Username = "Guest", Role = "Unregistered" };
            }
        }

        private static async Task RegisterAsync()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            try
            {
                var newUser = new UserDto
                {
                    Username = username,
                    Password = password,
                    Role = "Registered" // New users get Registered role by default
                };

                var createdUser = await _userService.CreateUserAsync(newUser);
                _currentUser = createdUser;
                Console.WriteLine($"Account created successfully. Welcome, {_currentUser.Username}!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration failed: {ex.Message}");
                _currentUser = new UserDto { Id = 0, Username = "Guest", Role = "Unregistered" };
            }
        }

        private static void Logout()
        {
            _currentUser = new UserDto { Id = 0, Username = "Guest", Role = "Unregistered" };
            Console.WriteLine("Logged out successfully.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        #endregion

        #region Products Menu
        private static async Task ProductsMenuAsync()
        {
            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== Products Menu ===");
                Console.WriteLine("1. View all products");
                Console.WriteLine("2. Search products");
                Console.WriteLine("3. View product details");

                if (_currentUser?.Role == "Admin" || _currentUser?.Role == "Manager")
                {
                    Console.WriteLine("4. Add new product");
                    Console.WriteLine("5. Edit product");
                    Console.WriteLine("6. Delete product");
                }

                Console.WriteLine("0. Back to main menu");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ViewAllProductsAsync();
                        break;
                    case "2":
                        await SearchProductsAsync();
                        break;
                    case "3":
                        await ViewProductDetailsAsync();
                        break;
                    case "4":
                        if (_currentUser?.Role == "Admin" || _currentUser?.Role == "Manager")
                            await AddProductAsync();
                        break;
                    case "5":
                        if (_currentUser?.Role == "Admin" || _currentUser?.Role == "Manager")
                            await EditProductAsync();
                        break;
                    case "6":
                        if (_currentUser?.Role == "Admin" || _currentUser?.Role == "Manager")
                            await DeleteProductAsync();
                        break;
                    case "0":
                        back = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static async Task ViewAllProductsAsync()
        {
            Console.Clear();
            Console.WriteLine("=== All Products ===");

            var products = await _productService.GetAllProductsAsync();
            DisplayProducts(products);

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task SearchProductsAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Search Products ===");
            Console.Write("Enter search term: ");
            string searchTerm = Console.ReadLine();

            var products = await _productService.SearchProductsAsync(searchTerm);
            DisplayProducts(products);

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task ViewProductDetailsAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Product Details ===");
            Console.Write("Enter product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid product ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                Console.WriteLine($"ID: {product.Id}");
                Console.WriteLine($"Name: {product.Name}");
                Console.WriteLine($"Price: ${product.Price}");
                Console.WriteLine($"Quantity in stock: {product.Quantity}");
                Console.WriteLine($"Description: {product.Description}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task AddProductAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Add New Product ===");

            var product = new ProductDto();

            Console.Write("Name: ");
            product.Name = Console.ReadLine();

            Console.Write("Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                Console.WriteLine("Invalid price.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            product.Price = price;

            Console.Write("Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity))
            {
                Console.WriteLine("Invalid quantity.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            product.Quantity = quantity;

            Console.Write("Description: ");
            product.Description = Console.ReadLine();

            try
            {
                var createdProduct = await _productService.CreateProductAsync(product);
                Console.WriteLine($"Product '{createdProduct.Name}' created successfully with ID: {createdProduct.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task EditProductAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Edit Product ===");
            Console.Write("Enter product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid product ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                Console.WriteLine($"Current Name: {product.Name}");
                Console.Write("New Name (leave empty to keep current): ");
                string name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name))
                    product.Name = name;

                Console.WriteLine($"Current Price: ${product.Price}");
                Console.Write("New Price (leave empty to keep current): ");
                string priceInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal price))
                    product.Price = price;

                Console.WriteLine($"Current Quantity: {product.Quantity}");
                Console.Write("New Quantity (leave empty to keep current): ");
                string quantityInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(quantityInput) && int.TryParse(quantityInput, out int quantity))
                    product.Quantity = quantity;

                Console.WriteLine($"Current Description: {product.Description}");
                Console.Write("New Description (leave empty to keep current): ");
                string description = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(description))
                    product.Description = description;

                var updatedProduct = await _productService.UpdateProductAsync(product);
                Console.WriteLine($"Product with ID {updatedProduct.Id} updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task DeleteProductAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Delete Product ===");
            Console.Write("Enter product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid product ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                Console.WriteLine($"You are about to delete product: {product.Name}");
                Console.Write("Are you sure? (y/n): ");
                string confirm = Console.ReadLine().ToLower();

                if (confirm == "y" || confirm == "yes")
                {
                    await _productService.DeleteProductAsync(id);
                    Console.WriteLine($"Product with ID {id} deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Deletion cancelled.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void DisplayProducts(IEnumerable<ProductDto> products)
        {
            if (!products.Any())
            {
                Console.WriteLine("No products found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("{0,-5} {1,-30} {2,-10} {3,-10}", "ID", "Name", "Price", "In Stock");
            Console.WriteLine(new string('-', 60));

            foreach (var product in products)
            {
                Console.WriteLine("{0,-5} {1,-30} {2,-10:C2} {3,-10}",
                    product.Id,
                    product.Name.Length > 27 ? product.Name.Substring(0, 27) + "..." : product.Name,
                    product.Price,
                    product.Quantity);
            }
        }
        #endregion

        #region Users Menu
        private static async Task UsersMenuAsync()
        {
            // Only Admin users can access the Users menu
            if (_currentUser?.Role != "Admin")
            {
                Console.WriteLine("Access denied. You need Admin privileges to access User management.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== Users Menu ===");
                Console.WriteLine("1. View all users");
                Console.WriteLine("2. View user details");
                Console.WriteLine("3. Change user role");
                Console.WriteLine("4. Delete user");
                Console.WriteLine("0. Back to main menu");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ViewAllUsersAsync();
                        break;
                    case "2":
                        await ViewUserDetailsAsync();
                        break;
                    case "3":
                        await ChangeUserRoleAsync();
                        break;
                    case "4":
                        await DeleteUserAsync();
                        break;
                    case "0":
                        back = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static async Task ViewAllUsersAsync()
        {
            Console.Clear();
            Console.WriteLine("=== All Users ===");

            var users = await _userService.GetAllUsersAsync();

            Console.WriteLine();
            Console.WriteLine("{0,-5} {1,-30} {2,-15}", "ID", "Username", "Role");
            Console.WriteLine(new string('-', 55));

            foreach (var user in users)
            {
                Console.WriteLine("{0,-5} {1,-30} {2,-15}",
                    user.Id,
                    user.Username.Length > 27 ? user.Username.Substring(0, 27) + "..." : user.Username,
                    user.Role);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task ViewUserDetailsAsync()
        {
            Console.Clear();
            Console.WriteLine("=== User Details ===");
            Console.Write("Enter user ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid user ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                Console.WriteLine($"ID: {user.Id}");
                Console.WriteLine($"Username: {user.Username}");
                Console.WriteLine($"Role: {user.Role}");

                // Get user orders
                var orders = await _orderService.GetUserOrdersAsync(id);
                Console.WriteLine($"Total Orders: {orders.Count()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task ChangeUserRoleAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Change User Role ===");
            Console.Write("Enter user ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid user ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                Console.WriteLine($"Current role for {user.Username}: {user.Role}");
                Console.WriteLine("Available roles: Admin, Manager, Registered, Unregistered");
                Console.Write("Enter new role: ");
                string newRole = Console.ReadLine();

                var updatedUser = await _userService.ChangeUserRoleAsync(id, newRole);
                Console.WriteLine($"Role for user {updatedUser.Username} changed to {updatedUser.Role}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task DeleteUserAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Delete User ===");
            Console.Write("Enter user ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid user ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Prevent deleting the current user
            if (id == _currentUser.Id)
            {
                Console.WriteLine("You cannot delete your own account while logged in.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                Console.WriteLine($"You are about to delete user: {user.Username}");
                Console.Write("Are you sure? (y/n): ");
                string confirm = Console.ReadLine().ToLower();

                if (confirm == "y" || confirm == "yes")
                {
                    await _userService.DeleteUserAsync(id);
                    Console.WriteLine($"User with ID {id} deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Deletion cancelled.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        #endregion

        #region Orders Menu
        private static async Task OrdersMenuAsync()
        {
            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== Orders Menu ===");

                if (_currentUser?.Role == "Admin" || _currentUser?.Role == "Manager")
                {
                    // Admin and Manager can see all orders
                    Console.WriteLine("1. View all orders");
                }

                if (_currentUser != null && _currentUser.Role != "Unregistered")
                {
                    // Registered users can see their own orders
                    Console.WriteLine("2. View my orders");
                }

                Console.WriteLine("3. View order details");

                if (_currentUser != null && _currentUser.Role != "Unregistered")
                {
                    // Only registered users can create orders
                    Console.WriteLine("4. Create new order");
                }

                Console.WriteLine("0. Back to main menu");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        if (_currentUser?.Role == "Admin" || _currentUser?.Role == "Manager")
                            await ViewAllOrdersAsync();
                        break;
                    case "2":
                        if (_currentUser != null && _currentUser.Role != "Unregistered")
                            await ViewMyOrdersAsync();
                        break;
                    case "3":
                        await ViewOrderDetailsAsync();
                        break;
                    case "4":
                        if (_currentUser != null && _currentUser.Role != "Unregistered")
                            await CreateOrderAsync();
                        break;
                    case "0":
                        back = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static async Task ViewAllOrdersAsync()
        {
            Console.Clear();
            Console.WriteLine("=== All Orders ===");

            var orders = await _orderService.GetAllOrdersAsync();
            DisplayOrders(orders);

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task ViewMyOrdersAsync()
        {
            Console.Clear();
            Console.WriteLine("=== My Orders ===");

            var orders = await _orderService.GetUserOrdersAsync(_currentUser.Id);
            DisplayOrders(orders);

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task ViewOrderDetailsAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Order Details ===");
            Console.Write("Enter order ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid order ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                // Check if current user has permission to view this order
                if (_currentUser.Role != "Admin" && _currentUser.Role != "Manager" && order.UserId != _currentUser.Id)
                {
                    Console.WriteLine("You don't have permission to view this order.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Order ID: {order.Id}");
                Console.WriteLine($"Customer: {order.Username}");
                Console.WriteLine($"Date: {order.CreatedAt}");
                Console.WriteLine($"Total: ${order.TotalAmount}");
                Console.WriteLine();
                Console.WriteLine("Items:");
                Console.WriteLine("{0,-5} {1,-30} {2,-10} {3,-10} {4,-10}", "ID", "Product", "Price", "Quantity", "Total");
                Console.WriteLine(new string('-', 70));

                foreach (var item in order.Items)
                {
                    Console.WriteLine("{0,-5} {1,-30} {2,-10:C2} {3,-10} {4,-10:C2}",
                        item.Id,
                        item.ProductName.Length > 27 ? item.ProductName.Substring(0, 27) + "..." : item.ProductName,
                        item.Price,
                        item.Quantity,
                        item.TotalPrice);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task CreateOrderAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Create New Order ===");

            var orderDto = new OrderDto
            {
                UserId = _currentUser.Id,
                CreatedAt = DateTime.Now,
                Items = new List<OrderItemDto>()
            };

            bool finishOrder = false;

            while (!finishOrder)
            {
                Console.Clear();
                Console.WriteLine("=== Create New Order ===");
                Console.WriteLine("Current items in cart:");

                if (orderDto.Items.Any())
                {
                    Console.WriteLine("{0,-30} {1,-10} {2,-10} {3,-10}", "Product", "Price", "Quantity", "Total");
                    Console.WriteLine(new string('-', 65));

                    foreach (var item in orderDto.Items)
                    {
                        Console.WriteLine("{0,-30} {1,-10:C2} {2,-10} {3,-10:C2}",
                            item.ProductName.Length > 27 ? item.ProductName.Substring(0, 27) + "..." : item.ProductName,
                            item.Price,
                            item.Quantity,
                            item.Price * item.Quantity);
                    }

                    Console.WriteLine(new string('-', 65));
                    Console.WriteLine($"Total: ${orderDto.TotalAmount}");
                }
                else
                {
                    Console.WriteLine("No items in cart");
                }

                Console.WriteLine();
                Console.WriteLine("1. Add product to cart");
                Console.WriteLine("2. Remove product from cart");
                Console.WriteLine("3. Checkout");
                Console.WriteLine("0. Cancel order");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await AddProductToOrderAsync(orderDto);
                        break;
                    case "2":
                        RemoveProductFromOrder(orderDto);
                        break;
                    case "3":
                        if (orderDto.Items.Any())
                        {
                            await CheckoutAsync(orderDto);
                            finishOrder = true;
                        }
                        else
                        {
                            Console.WriteLine("Cannot checkout with empty cart.");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        }
                        break;
                    case "0":
                        finishOrder = true;
                        Console.WriteLine("Order cancelled.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static async Task AddProductToOrderAsync(OrderDto orderDto)
        {
            Console.Clear();
            Console.WriteLine("=== Add Product to Order ===");

            // Show available products
            var products = await _productService.GetAllProductsAsync();
            Console.WriteLine("Available products:");
            DisplayProducts(products);

            Console.Write("Enter product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int productId))
            {
                Console.WriteLine("Invalid product ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(productId);

                Console.Write($"Enter quantity (max {product.Quantity}): ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Invalid quantity.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                if (quantity > product.Quantity)
                {
                    Console.WriteLine($"Not enough stock. Available: {product.Quantity}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Check if the product is already in the order
                var existingItem = orderDto.Items.FirstOrDefault(i => i.ProductId == product.Id);
                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Quantity += quantity;
                    existingItem.TotalPrice = existingItem.Price * existingItem.Quantity;
                }
                else
                {
                    // Add new item
                    var orderItem = new OrderItemDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = quantity,
                        TotalPrice = product.Price * quantity
                    };
                    orderDto.Items.Add(orderItem);
                }

                // Update order total
                orderDto.TotalAmount = orderDto.Items.Sum(i => i.TotalPrice);

                Console.WriteLine($"Added {quantity} x {product.Name} to cart.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void RemoveProductFromOrder(OrderDto orderDto)
        {
            Console.Clear();
            Console.WriteLine("=== Remove Product from Order ===");

            if (!orderDto.Items.Any())
            {
                Console.WriteLine("Cart is empty.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Display current items
            Console.WriteLine("Current items in cart:");
            Console.WriteLine("{0,-5} {1,-30} {2,-10} {3,-10} {4,-10}", "#", "Product", "Price", "Quantity", "Total");
            Console.WriteLine(new string('-', 70));

            for (int i = 0; i < orderDto.Items.Count; i++)
            {
                var item = orderDto.Items[i];
                Console.WriteLine("{0,-5} {1,-30} {2,-10:C2} {3,-10} {4,-10:C2}",
                    i + 1,
                    item.ProductName.Length > 27 ? item.ProductName.Substring(0, 27) + "..." : item.ProductName,
                    item.Price,
                    item.Quantity,
                    item.TotalPrice);
            }

            Console.Write("Enter item number to remove (or 0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int itemNumber) || itemNumber <= 0 || itemNumber > orderDto.Items.Count)
            {
                if (itemNumber != 0)
                    Console.WriteLine("Invalid item number.");
                else
                    Console.WriteLine("Removal cancelled.");

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Remove the item
            var removedItem = orderDto.Items[itemNumber - 1];
            orderDto.Items.RemoveAt(itemNumber - 1);

            // Update order total
            orderDto.TotalAmount = orderDto.Items.Sum(i => i.TotalPrice);

            Console.WriteLine($"Removed {removedItem.ProductName} from cart.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task CheckoutAsync(OrderDto orderDto)
        {
            Console.Clear();
            Console.WriteLine("=== Checkout ===");
            Console.WriteLine("Order Summary:");

            Console.WriteLine("{0,-30} {1,-10} {2,-10} {3,-10}", "Product", "Price", "Quantity", "Total");
            Console.WriteLine(new string('-', 65));

            foreach (var item in orderDto.Items)
            {
                Console.WriteLine("{0,-30} {1,-10:C2} {2,-10} {3,-10:C2}",
                    item.ProductName.Length > 27 ? item.ProductName.Substring(0, 27) + "..." : item.ProductName,
                    item.Price,
                    item.Quantity,
                    item.TotalPrice);
            }

            Console.WriteLine(new string('-', 65));
            Console.WriteLine($"Total: ${orderDto.TotalAmount}");

            Console.WriteLine();
            Console.Write("Confirm order (y/n): ");
            string confirm = Console.ReadLine().ToLower();

            if (confirm == "y" || confirm == "yes")
            {
                try
                {
                    // Create the order
                    var createdOrder = await _orderService.CreateOrderAsync(orderDto);

                    // Update product quantities
                    foreach (var item in orderDto.Items)
                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        product.Quantity -= item.Quantity;
                        await _productService.UpdateProductAsync(product);
                    }

                    Console.WriteLine($"Order placed successfully! Order ID: {createdOrder.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error placing order: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Order cancelled.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void DisplayOrders(IEnumerable<OrderDto> orders)
        {
            if (!orders.Any())
            {
                Console.WriteLine("No orders found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-15}", "ID", "User", "Date", "Total");
            Console.WriteLine(new string('-', 65));

            foreach (var order in orders)
            {
                Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-15:C2}",
                    order.Id,
                    order.Username.Length > 17 ? order.Username.Substring(0, 17) + "..." : order.Username,
                    order.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    order.TotalAmount);
            }
        }
        #endregion
    }
}