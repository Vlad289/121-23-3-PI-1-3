using AutoMapper;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services;
using DAL.Interfaces;
using DAL.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestProject
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly IMapper _mapper;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userRepoMock = new Mock<IUserRepository>();

            _unitOfWorkMock.SetupGet(u => u.Users).Returns(_userRepoMock.Object);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDto>().ReverseMap();
            });
            _mapper = config.CreateMapper();

            _service = new UserService(_unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_ValidInput_ChangesRole()
        {
            var user = new User { Id = 1, Role = "Registered" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.ChangeUserRoleAsync(1, "Admin");

            Assert.Equal("Admin", result.Role);
        }

        [Fact]
        public async Task CreateUserAsync_ValidInput_ReturnsUserDto()
        {
            var userDto = new UserDto { Username = "testuser", Password = "12345" };
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(Enumerable.Empty<User>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.CreateUserAsync(userDto);

            Assert.Equal(userDto.Username, result.Username);
            Assert.Null(result.Password);
        }

        [Fact]
        public async Task DeleteUserAsync_UserExists_DeletesUser()
        {
            var user = new User { Id = 1 };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await _service.DeleteUserAsync(1);

            _userRepoMock.Verify(r => r.Remove(user), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsUsers()
        {
            var users = new List<User> { new User { Id = 1, Username = "user1" } };
            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _service.GetAllUsersAsync();

            Assert.Single(result);
            Assert.Equal("user1", result.First().Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserExists_ReturnsUserDto()
        {
            var user = new User { Id = 1, Username = "user1" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _service.GetUserByIdAsync(1);

            Assert.Equal("user1", result.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_UserExists_ReturnsUserDto()
        {
            var users = new List<User> { new User { Id = 1, Username = "user1" } };
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(users);

            var result = await _service.GetUserByUsernameAsync("user1");

            Assert.Equal("user1", result.Username);
        }

        [Fact]
        public async Task UpdateUserAsync_UserExists_UpdatesUser()
        {
            var user = new User { Id = 1, Username = "oldname", PasswordHash = "oldhash" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var dto = new UserDto { Id = 1, Username = "newname", Password = "newpass" };

            var result = await _service.UpdateUserAsync(dto);

            Assert.Equal("newname", result.Username);
            Assert.Null(result.Password);
        }

        [Fact]
        public async Task ValidateUserAsync_ValidUser_ReturnsTrue()
        {
            var password = "password";
            var hashedPassword = _service.GetType()
                .GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_service, new object[] { password }) as string;
            var user = new User { Username = "user1", PasswordHash = hashedPassword };
            var users = new List<User> { user };

            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(users);

            var result = await _service.ValidateUserAsync("user1", password);

            Assert.True(result);
        }
    }
}
