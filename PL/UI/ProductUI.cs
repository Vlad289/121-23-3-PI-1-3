using BLL.DTOs;
using BLL.Exceptions;
using PL.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PL.UI
{
    /// <summary>
    /// UI handler for product operations
    /// </summary>
    public class ProductUI : BaseUI
    {
        private readonly ProductController _productController;

        public ProductUI(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _productController = GetController<ProductController>();
        }

        public override void ShowMenu()
        {
            bool exit = false;

            while (!exit)
            {
                DisplayHeader("Product Management");
                Console.WriteLine("1. View All Products");
                Console.WriteLine("2. View Product Details");
                Console.WriteLine("3. Search Products");
                Console.WriteLine("4. Create New Product");
                Console.WriteLine("5. Update Product");
                Console.WriteLine("6. Update Product Stock");
                Console.WriteLine("7. Delete Product");
                Console.WriteLine("0. Back to Main Menu");

                int choice = GetIntInput("\nEnter your choice: ", 0, 7);

                switch (choice)
                {
                    case 1:
                        ViewAllProductsAsync().Wait();
                        break;
                    case 2:
                        ViewProductDetailsAsync().Wait();
                        break;
                    case 3:
                        SearchProductsAsync().Wait();
                        break;
                    case 4:
                        CreateProductAsync().Wait();
                        break;
                    case 5:
                        UpdateProductAsync().Wait();
                        break;
                    case 6:
                        UpdateProductStockAsync().Wait();
                        break;
                    case 7:
                        DeleteProductAsync().Wait();
                        break;
                    case 0:
                        exit = true;
                        break;
                }
            }
        }

        private async Task ViewAllProductsAsync()
        {
            DisplayHeader("All Products");

            try
            {
                var products = await _productController.GetAllProductsAsync();
                DisplayProducts(products);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task ViewProductDetailsAsync()
        {
            DisplayHeader("Product Details");

            int productId = GetIntInput("Enter Product ID: ", 1);

            try
            {
                var product = await _productController.GetProductByIdAsync(productId);
                DisplayProductDetails(product);
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"Product with ID {productId} not found.");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task SearchProductsAsync()
        {
            DisplayHeader("Search Products");

            string searchTerm = GetStringInput("Enter search term: ");

            try
            {
                var products = await _productController.SearchProductsAsync(searchTerm);
                DisplayProducts(products);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task CreateProductAsync()
        {
            DisplayHeader("Create New Product");

            try
            {
                var productDto = new ProductDto();

                productDto.Name = GetStringInput("Enter product name: ");
                productDto.Description = GetStringInput("Enter product description: ", true);
                productDto.Price = GetDecimalInput("Enter price: $", 0.01m);
                productDto.Quantity = GetIntInput("Enter initial stock quantity: ", 0);

                var createdProduct = await _productController.CreateProductAsync(productDto);
                DisplaySuccess($"Product created with ID: {createdProduct.Id}");
                DisplayProductDetails(createdProduct);
            }
            catch (ValidationException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task UpdateProductAsync()
        {
            DisplayHeader("Update Product");

            int productId = GetIntInput("Enter Product ID: ", 1);

            try
            {
                var product = await _productController.GetProductByIdAsync(productId);
                DisplayProductDetails(product);

                Console.WriteLine("\nEnter new values (leave empty to keep current value):");

                string name = GetStringInput($"Name [{product.Name}]: ", true);
                if (!string.IsNullOrWhiteSpace(name))
                    product.Name = name;

                string description = GetStringInput($"Description [{product.Description}]: ", true);
                if (!string.IsNullOrWhiteSpace(description))
                    product.Description = description;

                string priceInput = GetStringInput($"Price [${product.Price}]: ", true);
                if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal price))
                    product.Price = price;

                // Note: We don't update quantity here, that's a separate operation

                var updatedProduct = await _productController.UpdateProductAsync(product);
                DisplaySuccess("Product updated successfully.");
                DisplayProductDetails(updatedProduct);
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"Product with ID {productId} not found.");
            }
            catch (ValidationException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task UpdateProductStockAsync()
        {
            DisplayHeader("Update Product Stock");

            int productId = GetIntInput("Enter Product ID: ", 1);

            try
            {
                var product = await _productController.GetProductByIdAsync(productId);
                DisplayProductDetails(product);

                Console.WriteLine("\nCurrent stock: " + product.Quantity);
                int quantityChange = GetIntInput("Enter quantity change (positive to add, negative to remove): ");

                if (quantityChange < 0 && Math.Abs(quantityChange) > product.Quantity)
                {
                    DisplayError("Cannot remove more than available stock.");
                    return;
                }

                await _productController.UpdateStockAsync(productId, quantityChange);
                DisplaySuccess($"Stock updated. New quantity: {product.Quantity + quantityChange}");
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"Product with ID {productId} not found.");
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

        private async Task DeleteProductAsync()
        {
            DisplayHeader("Delete Product");

            int productId = GetIntInput("Enter Product ID: ", 1);

            try
            {
                var product = await _productController.GetProductByIdAsync(productId);
                DisplayProductDetails(product);

                if (GetConfirmation("\nAre you sure you want to delete this product?"))
                {
                    await _productController.DeleteProductAsync(productId);
                    DisplaySuccess("Product deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Operation cancelled.");
                }
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"Product with ID {productId} not found.");
            }
            catch (InvalidOperationBusinessException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private void DisplayProducts(IEnumerable<ProductDto> products)
        {
            Console.WriteLine("\nProducts List:");
            Console.WriteLine($"{"ID",-5} {"Name",-30} {"Price",-10} {"Stock",-10}");
            Console.WriteLine(new string('-', 55));

            foreach (var product in products)
            {
                Console.WriteLine($"{product.Id,-5} {product.Name,-30} ${product.Price,-10:F2} {product.Quantity,-10}");
            }
        }

        private void DisplayProductDetails(ProductDto product)
        {
            Console.WriteLine($"\nProduct ID: {product.Id}");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"Description: {product.Description}");
            Console.WriteLine($"Price: ${product.Price:F2}");
            Console.WriteLine($"In Stock: {product.Quantity}");
        }
    }
}