using BLL.DTOs;
using BLL.Exceptions;
using PL.Controllers;

namespace PL.UI
{
    /// <summary>
    /// UI for product management
    /// </summary>
    public class ProductUI : BaseUI
    {
        private readonly ProductController _productController;
        private readonly CurrentUserManager _userManager;

        public ProductUI(IServiceProvider serviceProvider, CurrentUserManager userManager) : base(serviceProvider)
        {
            _productController = GetController<ProductController>();
            _userManager = userManager;
        }

        public override void ShowMenu()
        {
            bool back = false;

            while (!back)
            {
                DisplayHeader("Product Management");

                Console.WriteLine("1. List all products");
                Console.WriteLine("2. Find product by ID");
                Console.WriteLine("3. Search products");

                if (_userManager.IsAdmin)
                {
                    Console.WriteLine("4. Create new product");
                    Console.WriteLine("5. Update product");
                    Console.WriteLine("6. Delete product");
                    Console.WriteLine("7. Update product stock");
                }

                Console.WriteLine("0. Back to main menu");

                int maxChoice = _userManager.IsAdmin ? 7 : 3;
                int choice = GetIntInput("\nEnter your choice: ", 0, maxChoice);

                switch (choice)
                {
                    case 1:
                        ListAllProducts();
                        break;
                    case 2:
                        FindProductById();
                        break;
                    case 3:
                        SearchProducts();
                        break;
                    case 4:
                        if (_userManager.IsAdmin) CreateProduct();
                        break;
                    case 5:
                        if (_userManager.IsAdmin) UpdateProduct();
                        break;
                    case 6:
                        if (_userManager.IsAdmin) DeleteProduct();
                        break;
                    case 7:
                        if (_userManager.IsAdmin) UpdateProductStock();
                        break;
                    case 0:
                        back = true;
                        break;
                }
            }
        }

        private void ListAllProducts()
        {
            DisplayHeader("All Products");

            try
            {
                var products = _productController.GetAllProductsAsync().Result;

                if (!products.Any())
                {
                    Console.WriteLine("No products found.");
                }
                else
                {
                    DisplayProductList(products);
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to retrieve products: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void FindProductById()
        {
            DisplayHeader("Find Product by ID");

            int id = GetIntInput("Enter product ID: ", 1);

            try
            {
                var product = _productController.GetProductByIdAsync(id).Result;
                DisplayProductDetails(product);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private void SearchProducts()
        {
            DisplayHeader("Search Products");

            string searchTerm = GetStringInput("Enter search term: ");

            try
            {
                var products = _productController.SearchProductsAsync(searchTerm).Result;

                if (!products.Any())
                {
                    Console.WriteLine($"No products found matching '{searchTerm}'.");
                }
                else
                {
                    Console.WriteLine($"Found {products.Count()} products matching '{searchTerm}':");
                    DisplayProductList(products);
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Search failed: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void CreateProduct()
        {
            DisplayHeader("Create New Product");

            try
            {
                string name = GetStringInput("Enter product name: ");
                string description = GetStringInput("Enter product description: ");
                decimal price = GetDecimalInput("Enter product price: $", 0.01m);
                int stock = GetIntInput("Enter initial stock quantity: ", 0);

                var productDto = new ProductDto
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    StockQuantity = stock
                };

                var createdProduct = _productController.CreateProductAsync(productDto).Result;
                DisplaySuccess($"Product created successfully with ID: {createdProduct.Id}");
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to create product: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void UpdateProduct()
        {
            DisplayHeader("Update Product");

            int id = GetIntInput("Enter product ID to update: ", 1);

            try
            {
                var product = _productController.GetProductByIdAsync(id).Result;

                string name = GetStringInput($"Enter name [{product.Name}]: ", true);
                if (string.IsNullOrWhiteSpace(name)) name = product.Name;

                string description = GetStringInput($"Enter description [{product.Description}]: ", true);
                if (string.IsNullOrWhiteSpace(description)) description = product.Description;

                Console.Write($"Enter price [${product.Price:F2}] (leave empty to keep current): ");
                string priceInput = Console.ReadLine() ?? "";
                decimal price = string.IsNullOrWhiteSpace(priceInput) ? product.Price : decimal.Parse(priceInput);

                var updatedProductDto = new ProductDto
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    Price = price,
                    StockQuantity = product.StockQuantity
                };

                var updatedProduct = _productController.UpdateProductAsync(updatedProductDto).Result;
                DisplaySuccess("Product updated successfully.");
            }
            catch (FormatException)
            {
                DisplayError("Invalid price format. Product not updated.");
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to update product: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void DeleteProduct()
        {
            DisplayHeader("Delete Product");

            int id = GetIntInput("Enter product ID to delete: ", 1);

            try
            {
                var product = _productController.GetProductByIdAsync(id).Result;
                DisplayProductDetails(product);

                if (GetConfirmation("Are you sure you want to delete this product?"))
                {
                    _productController.DeleteProductAsync(id).Wait();
                    DisplaySuccess("Product deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Deletion canceled.");
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to delete product: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void UpdateProductStock()
        {
            DisplayHeader("Update Product Stock");

            int id = GetIntInput("Enter product ID: ", 1);

            try
            {
                var product = _productController.GetProductByIdAsync(id).Result;
                DisplayProductDetails(product);

                Console.WriteLine($"Current stock: {product.StockQuantity}");
                int quantityChange = GetIntInput("Enter quantity change (positive to add, negative to remove): ");

                _productController.UpdateStockAsync(id, quantityChange).Wait();

                var updatedProduct = _productController.GetProductByIdAsync(id).Result;
                DisplaySuccess($"Stock updated successfully. New stock: {updatedProduct.StockQuantity}");
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to update stock: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void DisplayProductList(IEnumerable<ProductDto> products)
        {
            Console.WriteLine($"{"ID",-5} {"Name",-30} {"Price",-10} {"Stock",-10}");
            Console.WriteLine(new string('-', 55));

            foreach (var product in products)
            {
                Console.WriteLine($"{product.Id,-5} {product.Name,-30} ${product.Price,-9:F2} {product.StockQuantity,-10}");
            }
        }

        private void DisplayProductDetails(ProductDto product)
        {
            Console.WriteLine("\nProduct Details:");
            Console.WriteLine($"ID: {product.Id}");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"Description: {product.Description}");
            Console.WriteLine($"Price: ${product.Price:F2}");
            Console.WriteLine($"Stock Quantity: {product.StockQuantity}");
        }
    }
}