using Fgc.Users.Application.Helpers;
using Fgc.Users.Application.Interfaces;
using Fgc.Users.Domain.Entities;
using Fgc.Users.Domain.Exceptions;
using Fgc.Users.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Fgc.MessageContracts.Events;

namespace Fgc.Users.Application.Services
{
    public class UserService(IUserRepository userRepository, ILogger<UserService> logger, IPublishEndpoint publishEndpoint, IEventLogRepository eventLogRepository, IDistributedCache cache)
    {

        public async Task<User> RegisterAsync(string name,string email,string password)
        {
            // Verificar se o email já está registrado. Procurando remover o Entity Framework que estava instalado por ocasião do monolito.
            var emailExists = await userRepository.ExistsByEmailAsync(email);

            if (emailExists)
            {
                throw new ConflictException("Email already registered.");
            }

            PasswordValidator.Validate(password);

            var passwordHash = PasswordHasher.Hash(password);
            var emailVo = Email.Create(email);

            var user = User.Create(
                name,
                emailVo,
                passwordHash
            );
            await userRepository.AddAsync(user);

            var userCreatedEvent = new UserCreatedEvent(
                    user.Id,
                    user.Name,
                    user.Email.Value,
                    DateTime.UtcNow
                );

            await publishEndpoint.Publish(userCreatedEvent);

            await eventLogRepository.LogAsync("UserCreatedEvent", userCreatedEvent, CancellationToken.None);

            await cache.RemoveAsync(AdminUserService.AllUsersCacheKey);

            logger.LogInformation(
                "User registered successfully | UserId={UserId} | Email={Email}",
                user.Id,
                user.Email.Value
            );

            return user;
        }

        public async Task<User> CreateFirstAdminAsync(string name, string email, string password)
        {
            if (await userRepository.AnyAdminExistsAsync())
            {
                throw new ConflictException("An admin user already exists.");
            }

            var emailExists = await userRepository.ExistsByEmailAsync(email);

            if (emailExists)
            {
                throw new ConflictException("Email already registered.");
            }

            PasswordValidator.Validate(password);

            var passwordHash = PasswordHasher.Hash(password);
            var emailVo = Email.Create(email);

            var user = User.Create(
                name,
                emailVo,
                passwordHash,
                "Admin"
            );
            await userRepository.AddAsync(user);

            var userCreatedEvent = new UserCreatedEvent(
                    user.Id,
                    user.Name,
                    user.Email.Value,
                    DateTime.UtcNow
                );

            await publishEndpoint.Publish(userCreatedEvent);

            await eventLogRepository.LogAsync("UserCreatedEvent", userCreatedEvent, CancellationToken.None);

            await cache.RemoveAsync(AdminUserService.AllUsersCacheKey);

            logger.LogInformation(
                "First admin user created successfully | UserId={UserId} | Email={Email}",
                user.Id,
                user.Email.Value
            );

            return user;
        }

        public async Task<User> UpdateUserAsync(
            Guid userId,
            string email,
            string password)
        {
            var user = await userRepository.GetByIdAsync(userId);

            if (user is null)
            {
               logger.LogWarning("Attempt to update non-existing user | UserId={UserId}", userId);

                throw new NotFoundException("User not found.");
            }

            PasswordValidator.Validate(password);

            user.UpdateEmail(Email.Create(email));
            user.UpdatePassword(PasswordHasher.Hash(password));

            await userRepository.UpdateAsync(user);

            await cache.RemoveAsync(AdminUserService.AllUsersCacheKey);

            return user;
        }
    }
}
