using Fgc.Users.Application.Interfaces;
using Fgc.Users.Application.Services;
using Fgc.Users.Domain.Entities;
using Fgc.Users.Domain.Exceptions;
using Fgc.Users.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Fgc.Users.Tests.Services
{
    public class AdminUserServiceTests
    {
        private readonly Mock<IAdminUserRepository> _adminUserRepositoryMock;
        private readonly Mock<ILogger<AdminUserService>> _loggerMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly IDistributedCache _cache;
        private readonly AdminUserService _adminUserService;

        public AdminUserServiceTests()
        {
            _adminUserRepositoryMock = new Mock<IAdminUserRepository>();
            _loggerMock = new Mock<ILogger<AdminUserService>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            _adminUserService = new AdminUserService(_adminUserRepositoryMock.Object, _loggerMock.Object, _publishEndpointMock.Object, _cache);
        }

        private static User CreateUser(string name = "User Test", string email = "user@test.com", string role = "User")
        {
            return User.Create(name, Email.Create(email), "hash", role);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ShouldHitRepository_WhenCacheIsEmpty()
        {
            var users = new List<User> { CreateUser() };
            _adminUserRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _adminUserService.GetAllAsync();

            Assert.Single(result);
            _adminUserRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldNotHitRepository_WhenCalledTwice()
        {
            var users = new List<User> { CreateUser() };
            _adminUserRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            await _adminUserService.GetAllAsync();
            await _adminUserService.GetAllAsync();

            _adminUserRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        #endregion

        #region RegisterAsync

        [Fact]
        public async Task RegisterAsync_ShouldInvalidateCache()
        {
            var users = new List<User> { CreateUser() };
            _adminUserRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            await _adminUserService.GetAllAsync();

            _adminUserRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            await _adminUserService.RegisterAsync("New User", "new@test.com", "Senha@123");

            await _adminUserService.GetAllAsync();

            _adminUserRepositoryMock.Verify(r => r.GetAllAsync(), Times.Exactly(2));
        }

        #endregion

        #region UpdateByAdminAsync

        [Fact]
        public async Task UpdateByAdminAsync_ShouldInvalidateCache()
        {
            var user = CreateUser();
            var users = new List<User> { user };
            _adminUserRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            await _adminUserService.GetAllAsync();

            _adminUserRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _adminUserRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            await _adminUserService.UpdateByAdminAsync(user.Id, "Admin");

            await _adminUserService.GetAllAsync();

            _adminUserRepositoryMock.Verify(r => r.GetAllAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateByAdminAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            _adminUserRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _adminUserService.UpdateByAdminAsync(Guid.NewGuid(), "Admin"));
        }

        #endregion

        #region DeleteUserAsync

        [Fact]
        public async Task DeleteUserAsync_ShouldInvalidateCache()
        {
            var user = CreateUser();
            var users = new List<User> { user };
            _adminUserRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            await _adminUserService.GetAllAsync();

            _adminUserRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _adminUserRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            await _adminUserService.DeleteUserAsync(user.Id);

            await _adminUserService.GetAllAsync();

            _adminUserRepositoryMock.Verify(r => r.GetAllAsync(), Times.Exactly(2));
        }

        #endregion
    }
}
