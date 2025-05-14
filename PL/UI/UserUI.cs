// PL/UI/UserUI.cs - Complete implementation
using BLL.DTOs;
using BLL.Exceptions;
using PL.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PL.UI
{
    /// <summary>
    /// UI handler for user operations
    /// </summary>
    public class UserUI : BaseUI
    {
        private readonly UserController _userController;

        public UserUI(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userController = GetController<UserController>();
        }

        public override void ShowMenu()
        {
            bool exit = false;

            while (!exit)
            {
                DisplayHeader("User Management");
                Console.WriteLine("1. View All Users");
                Console.WriteLine("2. View User Details");
                Console.WriteLine("3. Find User by Username");
                Console.WriteLine("4. Create New User");
                Console.WriteLine("5. Update User");
                Console.WriteLine("6. Change User Role");
                Console.WriteLine("7. Delete User");
                Console.WriteLine("0. Back to Main Menu");

                int choice = GetIntInput("\nEnter your choice: ", 0, 7);

                switch (choice)
                {
                    case 1:
                        ViewAllUsersAsync().Wait();
                        break;
                    case 2:
                        ViewUserDetailsAsync().Wait();
                        break;
                    case 3:
                        FindUserByUsernameAsync().Wait();
                        break;
                    case 4:
                        CreateUserAsync().Wait();
                        break;
                    case 5:
                        UpdateUserAsync().Wait();
                        break;
                    case 6:
                        ChangeUserRoleAsync().Wait();
                        break;
                    case 7:
                        DeleteUserAsync().Wait();
                        break;
                    case 0:
                        exit = true;
                        break;
                }
            }
        }

        private async Task ViewAllUsersAsync()
        {
            DisplayHeader("All Users");

            try
            {
                var users = await _userController.GetAllUsersAsync();
                DisplayUsers(users);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task ViewUserDetailsAsync()
        {
            DisplayHeader("User Details");

            int userId = GetIntInput("Enter User ID: ", 1);

            try
            {
                var user = await _userController.GetUserByIdAsync(userId);
                DisplayUserDetails(user);
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

        private async Task FindUserByUsernameAsync()
        {
            DisplayHeader("Find User by Username");

            string username = GetStringInput("Enter username: ");

            try
            {
                var user = await _userController.GetUserByUsernameAsync(username);
                DisplayUserDetails(user);
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"User with username '{username}' not found.");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private async Task CreateUserAsync()
        {
            DisplayHeader("Create New User");

            try
            {
                var userDto = new UserDto();

                userDto.Username = GetStringInput("Enter username: ");
                userDto.Email = GetStringInput("Enter email: ");
                userDto.Password = GetStringInput("Enter password: ");
                userDto.FullName = GetStringInput("Enter full name: ");

                Console.WriteLine("\nSelect Role:");
                Console.WriteLine("1. Customer");
                Console.WriteLine("2. Admin");

                int roleChoice = GetIntInput("Enter choice: ", 1, 2);
                userDto.Role = roleChoice == 1 ? "Customer" : "Admin";

                var createdUser = await _userController.CreateUserAsync(userDto);
                DisplaySuccess($"User created with ID: {createdUser.Id}");
                DisplayUserDetails(createdUser);
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

        private async Task UpdateUserAsync()
        {
            DisplayHeader("Update User");

            int userId = GetIntInput("Enter User ID: ", 1);

            try
            {
                var user = await _userController.GetUserByIdAsync(userId);
                DisplayUserDetails(user);

                Console.WriteLine("\nEnter new values (leave empty to keep current value):");

                string username = GetStringInput($"Username [{user.Username}]: ", true);
                if (!string.IsNullOrWhiteSpace(username))
                    user.Username = username;

                string email = GetStringInput($"Email [{user.Email}]: ", true);
                if (!string.IsNullOrWhiteSpace(email))
                    user.Email = email;

                string password = GetStringInput("Enter new password (leave empty to keep current): ", true);
                if (!string.IsNullOrWhiteSpace(password))
                    user.Password = password;

                string fullName = GetStringInput($"Full Name [{user.FullName}]: ", true);
                if (!string.IsNullOrWhiteSpace(fullName))
                    user.FullName = fullName;

                var updatedUser = await _userController.UpdateUserAsync(user);
                DisplaySuccess("User updated successfully.");
                DisplayUserDetails(updatedUser);
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

        private async Task ChangeUserRoleAsync()
        {
            DisplayHeader("Change User Role");

            int userId = GetIntInput("Enter User ID: ", 1);

            try
            {
                var user = await _userController.GetUserByIdAsync(userId);
                DisplayUserDetails(user);

                Console.WriteLine("\nSelect New Role:");
                Console.WriteLine("1. Customer");
                Console.WriteLine("2. Admin");

                int roleChoice = GetIntInput("Enter choice: ", 1, 2);
                string newRole = roleChoice == 1 ? "Customer" : "Admin";

                if (newRole == user.Role)
                {
                    Console.WriteLine($"User already has the role '{newRole}'.");
                    PressAnyKeyToContinue();
                    return;
                }

                var updatedUser = await _userController.ChangeUserRoleAsync(userId, newRole);
                DisplaySuccess($"User role changed to {newRole} successfully.");
                DisplayUserDetails(updatedUser);
            }
            catch (EntityNotFoundException)
            {
                DisplayError($"User with ID {userId} not found.");
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

        private async Task DeleteUserAsync()
        {
            DisplayHeader("Delete User");

            int userId = GetIntInput("Enter User ID: ", 1);

            try
            {
                var user = await _userController.GetUserByIdAsync(userId);
                DisplayUserDetails(user);

                if (GetConfirmation("\nAre you sure you want to delete this user?"))
                {
                    await _userController.DeleteUserAsync(userId);
                    DisplaySuccess("User deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Operation cancelled.");
                }
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

        private void DisplayUsers(IEnumerable<UserDto> users)
        {
            Console.WriteLine("\nUsers List:");
            Console.WriteLine($"{"ID",-5} {"Username",-20} {"Email",-30} {"Role",-10}");
            Console.WriteLine(new string('-', 65));

            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id,-5} {user.Username,-20} {user.Email,-30} {user.Role,-10}");
            }
        }

        private void DisplayUserDetails(UserDto user)
        {
            Console.WriteLine($"\nUser ID: {user.Id}");
            Console.WriteLine($"Username: {user.Username}");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Full Name: {user.FullName}");
            Console.WriteLine($"Role: {user.Role}");
            Console.WriteLine($"Created: {user.CreatedAt.ToString("yyyy-MM-dd HH:mm")}");
        }
    }
}