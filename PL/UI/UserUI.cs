using BLL.DTOs;
using BLL.Exceptions;
using PL.Controllers;

namespace PL.UI
{
    /// <summary>
    /// UI for user management
    /// </summary>
    public class UserUI : BaseUI
    {
        private readonly UserController _userController;
        private readonly CurrentUserManager _userManager;

        public UserUI(IServiceProvider serviceProvider, CurrentUserManager userManager) : base(serviceProvider)
        {
            _userController = GetController<UserController>();
            _userManager = userManager;
        }

        public override void ShowMenu()
        {
            bool back = false;

            while (!back)
            {
                DisplayHeader("User Management");

                if (_userManager.IsAdmin)
                {
                    Console.WriteLine("1. List all users");
                    Console.WriteLine("2. Find user by ID");
                    Console.WriteLine("3. Find user by username");
                    Console.WriteLine("4. Create new user");
                    Console.WriteLine("5. Update user");
                    Console.WriteLine("6. Delete user");
                    Console.WriteLine("7. Change user role");
                    Console.WriteLine("0. Back to main menu");

                    int choice = GetIntInput("\nEnter your choice: ", 0, 7);

                    switch (choice)
                    {
                        case 1:
                            ListAllUsers();
                            break;
                        case 2:
                            FindUserById();
                            break;
                        case 3:
                            FindUserByUsername();
                            break;
                        case 4:
                            CreateUser();
                            break;
                        case 5:
                            UpdateUser();
                            break;
                        case 6:
                            DeleteUser();
                            break;
                        case 7:
                            ChangeUserRole();
                            break;
                        case 0:
                            back = true;
                            break;
                    }
                }
                else
                {
                    // Regular user can only see and update their profile
                    Console.WriteLine("1. View my profile");
                    Console.WriteLine("2. Update my profile");
                    Console.WriteLine("0. Back to main menu");

                    int choice = GetIntInput("\nEnter your choice: ", 0, 2);

                    switch (choice)
                    {
                        case 1:
                            DisplayUserDetails(_userManager.CurrentUser!);
                            PressAnyKeyToContinue();
                            break;
                        case 2:
                            UpdateCurrentUser();
                            break;
                        case 0:
                            back = true;
                            break;
                    }
                }
            }
        }

        private void ListAllUsers()
        {
            DisplayHeader("All Users");

            try
            {
                var users = _userController.GetAllUsersAsync().Result;

                if (!users.Any())
                {
                    Console.WriteLine("No users found.");
                }
                else
                {
                    Console.WriteLine($"{"ID",-5} {"Username",-20} {"Email",-30} {"Full Name",-30} {"Role",-10}");
                    Console.WriteLine(new string('-', 95));

                    foreach (var user in users)
                    {
                        Console.WriteLine($"{user.Id,-5} {user.Username,-20} {user.Email,-30} {user.FullName,-30} {user.Role,-10}");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to retrieve users: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void FindUserById()
        {
            DisplayHeader("Find User by ID");

            int id = GetIntInput("Enter user ID: ", 1);

            try
            {
                var user = _userController.GetUserByIdAsync(id).Result;
                DisplayUserDetails(user);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private void FindUserByUsername()
        {
            DisplayHeader("Find User by Username");

            string username = GetStringInput("Enter username: ");

            try
            {
                var user = _userController.GetUserByUsernameAsync(username).Result;
                DisplayUserDetails(user);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }

            PressAnyKeyToContinue();
        }

        private void CreateUser()
        {
            DisplayHeader("Create New User");

            try
            {
                string username = GetStringInput("Enter username: ");
                string email = GetStringInput("Enter email: ");
                string password = GetStringInput("Enter password: ");
                string fullName = GetStringInput("Enter full name: ");
                string role = GetStringInput("Enter role (Admin/Customer): ");

                var userDto = new UserDto
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    FullName = fullName,
                    Role = role
                };

                var createdUser = _userController.CreateUserAsync(userDto).Result;
                DisplaySuccess($"User created successfully with ID: {createdUser.Id}");
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to create user: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void UpdateUser()
        {
            DisplayHeader("Update User");

            int id = GetIntInput("Enter user ID to update: ", 1);

            try
            {
                var user = _userController.GetUserByIdAsync(id).Result;
                
                string username = GetStringInput($"Enter username [{user.Username}]: ", true);
                if (string.IsNullOrWhiteSpace(username)) username = user.Username;
                
                string email = GetStringInput($"Enter email [{user.Email}]: ", true);
                if (string.IsNullOrWhiteSpace(email)) email = user.Email;
                
                string password = GetStringInput("Enter new password (leave empty to keep current): ", true);
                
                string fullName = GetStringInput($"Enter full name [{user.FullName}]: ", true);
                if (string.IsNullOrWhiteSpace(fullName)) fullName = user.FullName;

                var updatedUserDto = new UserDto
                {
                    Id = id,
                    Username = username,
                    Email = email,
                    FullName = fullName,
                    Role = user.Role
                };

                if (!string.IsNullOrWhiteSpace(password))
                {
                    updatedUserDto.Password = password;
                }
                else
                {
                    updatedUserDto.Password = user.Password;
                }

                var updatedUser = _userController.UpdateUserAsync(updatedUserDto).Result;
                DisplaySuccess("User updated successfully.");
                
                // If current user, update the session
                if (_userManager.CurrentUser?.Id == updatedUser.Id)
                {
                    _userManager.Login(updatedUser);
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to update user: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void UpdateCurrentUser()
        {
            DisplayHeader("Update My Profile");

            try
            {
                var user = _userManager.CurrentUser!;
                
                string email = GetStringInput($"Enter email [{user.Email}]: ", true);
                if (string.IsNullOrWhiteSpace(email)) email = user.Email;
                
                string password = GetStringInput("Enter new password (leave empty to keep current): ", true);
                
                string fullName = GetStringInput($"Enter full name [{user.FullName}]: ", true);
                if (string.IsNullOrWhiteSpace(fullName)) fullName = user.FullName;

                var updatedUserDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = email,
                    FullName = fullName,
                    Role = user.Role
                };

                if (!string.IsNullOrWhiteSpace(password))
                {
                    updatedUserDto.Password = password;
                }
                else
                {
                    updatedUserDto.Password = user.Password;
                }

                var updatedUser = _userController.UpdateUserAsync(updatedUserDto).Result;
                DisplaySuccess("Profile updated successfully.");
                
                // Update the session
                _userManager.Login(updatedUser);
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to update profile: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void DeleteUser()
        {
            DisplayHeader("Delete User");

            int id = GetIntInput("Enter user ID to delete: ", 1);

            try
            {
                if (id == _userManager.CurrentUser?.Id)
                {
                    DisplayError("You cannot delete your own account while logged in.");
                    PressAnyKeyToContinue();
                    return;
                }

                var user = _userController.GetUserByIdAsync(id).Result;
                DisplayUserDetails(user);

                if (GetConfirmation("Are you sure you want to delete this user?"))
                {
                    _userController.DeleteUserAsync(id).Wait();
                    DisplaySuccess("User deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Deletion canceled.");
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to delete user: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void ChangeUserRole()
        {
            DisplayHeader("Change User Role");

            int id = GetIntInput("Enter user ID: ", 1);

            try
            {
                var user = _userController.GetUserByIdAsync(id).Result;
                DisplayUserDetails(user);

                string newRole = GetStringInput("Enter new role (Admin/Customer): ");

                var updatedUser = _userController.ChangeUserRoleAsync(id, newRole).Result;
                DisplaySuccess($"User role changed successfully to {updatedUser.Role}.");
                
                // If current user, update the session
                if (_userManager.CurrentUser?.Id == updatedUser.Id)
                {
                    _userManager.Login(updatedUser);
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to change user role: {ex.Message}");
            }

            PressAnyKeyToContinue();
        }

        private void DisplayUserDetails(UserDto user)
        {
            Console.WriteLine("\nUser Details:");
            Console.WriteLine($"ID: {user.Id}");
            Console.WriteLine($"Username: {user.Username}");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Full Name: {user.FullName}");
            Console.WriteLine($"Role: {user.Role}");
        }
    }
}