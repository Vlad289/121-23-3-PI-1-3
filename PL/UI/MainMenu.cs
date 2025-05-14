using DAL.Models;
using PL.Controllers;

namespace PL.UI
{
    /// <summary>
    /// Main menu UI class
    /// </summary>
    public class MainMenuUI : BaseUI
    {
        private readonly UserUI _userUI;
        private readonly ProductUI _productUI;
        private readonly OrderUI _orderUI;
        private readonly CurrentUserManager _userManager;

        public MainMenuUI(
            IServiceProvider serviceProvider,
        UserUI userUI,
            ProductUI productUI,
            OrderUI orderUI,
            CurrentUserManager userManager) : base(serviceProvider)
        {
            _userUI = userUI;
            _productUI = productUI;
            _orderUI = orderUI;
            _userManager = userManager;
        }

        public override void ShowMenu()
        {
            bool exit = false;

            while (!exit)
            {
                DisplayHeader("Online Shop Management System");

                if (_userManager.IsLoggedIn)
                {
                    Console.WriteLine($"Logged in as: {_userManager.CurrentUser!.Username} ({_userManager.CurrentUser.Role})");
                }
                else
                {
                    Console.WriteLine("Not logged in");
                }

                Console.WriteLine("\nMain Menu:");
                Console.WriteLine("1. User Management");
                Console.WriteLine("2. Product Management");
                Console.WriteLine("3. Order Management");

                if (!_userManager.IsLoggedIn)
                {
                    Console.WriteLine("4. Login");
                }
                else
                {
                    Console.WriteLine("4. Logout");
                }

                Console.WriteLine("0. Exit");

                int choice = GetIntInput("\nEnter your choice: ", 0, 4);

                switch (choice)
                {
                    case 1:
                        // Only admin or logged in users for their own profile
                        if (_userManager.IsAdmin || _userManager.IsLoggedIn)
                        {
                            _userUI.ShowMenu();
                        }
                        else
                        {
                            DisplayError("You need to log in first.");
                            PressAnyKeyToContinue();
                        }
                        break;
                    case 2:
                        _productUI.ShowMenu();
                        break;
                    case 3:
                        if (_userManager.IsLoggedIn)
                        {
                            _orderUI.ShowMenu();
                        }
                        else
                        {
                            DisplayError("You need to log in first.");
                            PressAnyKeyToContinue();
                        }
                        break;
                    case 4:
                        if (_userManager.IsLoggedIn)
                        {
                            _userManager.Logout();
                            DisplaySuccess("Logged out successfully.");
                        }
                        else
                        {
                            HandleLogin();
                        }
                        PressAnyKeyToContinue();
                        break;
                    case 0:
                        exit = true;
                        Console.WriteLine("Thank you for using Online Shop Management System. Goodbye!");
                        break;
                }
            }
        }

        private void HandleLogin()
        {
            DisplayHeader("Login");

            string username = GetStringInput("Enter username: ");
            string password = GetStringInput("Enter password: ");

            try
            {
                var userController = GetController<UserController>();
                bool isValid = userController.ValidateUserAsync(username, password).Result;

                if (isValid)
                {
                    var user = userController.GetUserByUsernameAsync(username).Result;
                    _userManager.Login(user);
                    DisplaySuccess($"Welcome, {user.Username}!");
                }
                else
                {
                    DisplayError("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Login failed: {ex.Message}");
            }
        }
    }
}