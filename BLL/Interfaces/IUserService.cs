using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(int id);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<UserDto> CreateUserAsync(UserDto userDto);
        Task<UserDto> UpdateUserAsync(UserDto userDto);
        Task DeleteUserAsync(int id);
        Task<bool> ValidateUserAsync(string username, string password);
        Task<UserDto> ChangeUserRoleAsync(int id, string newRole);
    }
}