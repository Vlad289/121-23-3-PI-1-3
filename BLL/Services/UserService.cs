using AutoMapper;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Interfaces;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto> ChangeUserRoleAsync(int id, string newRole)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("User", id);

            var validRoles = new[] { "Admin", "Manager", "Registered", "Unregistered" };
            if (!validRoles.Contains(newRole))
                throw new ValidationException($"Invalid role: {newRole}. Valid roles are: {string.Join(", ", validRoles)}");

            user.Role = newRole;
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Username))
                throw new ValidationException("Username cannot be empty");

            if (string.IsNullOrWhiteSpace(userDto.Password))
                throw new ValidationException("Password cannot be empty");

            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Username == userDto.Username);
            if (existingUsers.Any())
                throw new ValidationException($"Username '{userDto.Username}' is already taken");

            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = HashPassword(userDto.Password);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var createdUser = _mapper.Map<UserDto>(user);
            createdUser.Password = null; // Don't return password
            return createdUser;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("User", id);

            _unitOfWork.Users.Remove(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("User", id);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault()
                ?? throw new EntityNotFoundException($"User with username '{username}' was not found.");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUserAsync(UserDto userDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userDto.Id)
                ?? throw new EntityNotFoundException("User", userDto.Id);

            // Update properties
            user.Username = userDto.Username;

            // Only update password if a new one is provided
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                user.PasswordHash = HashPassword(userDto.Password);
            }

            // Don't update role here - use ChangeUserRoleAsync for that

            await _unitOfWork.SaveChangesAsync();

            var updatedUser = _mapper.Map<UserDto>(user);
            updatedUser.Password = null; // Don't return password
            return updatedUser;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault();

            if (user == null)
                return false;

            return user.PasswordHash == HashPassword(password);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}