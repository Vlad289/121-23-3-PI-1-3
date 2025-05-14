using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;

namespace PL.Controllers
{
    /// <summary>
    /// Controller for handling user operations
    /// </summary>
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userService = GetService<IUserService>();
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _userService.GetAllUsersAsync();
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            try
            {
                return await _userService.GetUserByIdAsync(id);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _userService.GetUserByUsernameAsync(username);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            try
            {
                return await _userService.CreateUserAsync(userDto);
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<UserDto> UpdateUserAsync(UserDto userDto)
        {
            try
            {
                return await _userService.UpdateUserAsync(userDto);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a user by ID
        /// </summary>
        public async Task DeleteUserAsync(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validate user credentials
        /// </summary>
        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            return await _userService.ValidateUserAsync(username, password);
        }

        /// <summary>
        /// Change user role
        /// </summary>
        public async Task<UserDto> ChangeUserRoleAsync(int id, string newRole)
        {
            try
            {
                return await _userService.ChangeUserRoleAsync(id, newRole);
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                throw;
            }
        }
    }
}