using Fgc.MessageContracts.Events;
using Fgc.Users.Application.Interfaces;
using Fgc.Users.Application.Services;
using Fgc.Users.Domain.Entities;
using Fgc.Users.Domain.Exceptions;
using Fgc.Users.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fgc.Users.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object, _publishEndpointMock.Object);
        }

        #region RegisterAsync

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenDataIsValid()
        {
            // Arrange
            var name = "User Test";
            var email = "user@test.com";
            var password = "Senha@123";

            _userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterAsync(name, email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(email, result.Email.Value);

            _userRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<User>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
        {
            // Arrange
            var duplicatedEmail = "duplicated@test.com";

            // Simula que o email já existe no repositório
            _userRepositoryMock
                .Setup(r => r.ExistsByEmailAsync(duplicatedEmail))
                .ReturnsAsync(true);

            // Act & Assert
            // Tenta registrar um novo usuário com o email duplicado e verifica se a exceção de conflito é lançada
            await Assert.ThrowsAsync<ConflictException>(() =>
                _userService.RegisterAsync(
                    "New User",
                    duplicatedEmail,
                    "Password@123"));
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenPasswordIsInvalid()
        {
            // Arrange
            var name = "User Test";
            var email = "user@test.com";
            var invalidPassword = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.RegisterAsync(
                    name,
                    email,
                    invalidPassword));
        }

        [Fact]
        public async Task RegisterAsync_ShouldPublishUserCreatedEvent_WhenRegistrationSucceds()
        {
            // Arrange
            var name = "João Silva";
            var email = "joao@dominio.com";
            var password = "P@ssword@123";

            _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(email))
                .ReturnsAsync(false);

            // Act
            var user = await _userService.RegisterAsync(name, email, password);

            // Assert
            _publishEndpointMock.Verify(
                p => p.Publish(
                    It.Is<UserCreatedEvent>(e => 
                        e.Id == user.Id &&
                        e.Name == name && 
                        e.Email == email),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        #endregion

        #region UpdateUserAsync

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.Create(
                "User",
                Email.Create("old@test.com"),
                "oldHash");

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.UpdateAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserAsync(
                userId,
                "new@test.com",
                "NovaSenha@123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new@test.com", result.Email.Value);

            _userRepositoryMock.Verify(
                r => r.UpdateAsync(user),
                Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _userService.UpdateUserAsync(
                    Guid.NewGuid(),
                    "email@test.com",
                    "Senha@123"));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowArgumentException_WhenPasswordIsInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.Create(
                "User",
                Email.Create("user@test.com"),
                "hash");

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.UpdateUserAsync(
                    userId,
                    "user@test.com",
                    ""));
        }

        #endregion
    }
}

