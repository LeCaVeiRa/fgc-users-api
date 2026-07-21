using Fgc.MessageContracts.Events;
using Fgc.Users.Application.Helpers;
using Fgc.Users.Application.Interfaces;
using Fgc.Users.Domain.Entities;
using Fgc.Users.Domain.Exceptions;
using Fgc.Users.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Fgc.Users.Application.Services
{
    public class AdminUserService(IAdminUserRepository adminUserRepository, ILogger<AdminUserService> logger, IPublishEndpoint publishEndpoint)
    {
        
        public async Task<User> RegisterAsync(
            string name,
            string email,
            string password)
        {
            PasswordValidator.Validate(password);

            var passwordHash = PasswordHasher.Hash(password);
            var emailVo = Email.Create(email);

            var user = User.Create(
                name,
                emailVo,
                passwordHash
            );

            await adminUserRepository.AddAsync(user);

            var userCreatedEvent = new UserCreatedEvent(user.Id, user.Name, user.Email.Value, DateTime.UtcNow);
            await publishEndpoint.Publish(userCreatedEvent);

            logger.LogInformation("Admin created user | UserId={UserId} | Email={Email}", user.Id,user.Email.Value);

            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            logger.LogInformation("Admin requested user list");

            return await adminUserRepository.GetAllAsync();
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            logger.LogInformation("Admin requested user details | UserId={UserId}",id);

            return await adminUserRepository.GetByIdAsync(id);
        }

        public async Task<User> UpdateByAdminAsync(Guid userId, string role)
        {
            var user = await adminUserRepository.GetByIdAsync(userId);

            if (user is null)
            {
                logger.LogWarning(
                    "Admin attempted to update role of non-existing user | UserId={UserId}",
                    userId
                );
                throw new NotFoundException("User not found.");
            }

            if (role != "User" && role != "Admin")
                throw new ArgumentException("Invalid role.");

            user.UpdateRole(role);

            await adminUserRepository.UpdateAsync(user);

            logger.LogInformation(
                "Admin updated user role | UserId={UserId} | NewRole={Role}",
                user.Id,
                role
            );

            return user;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await adminUserRepository.GetByIdAsync(id);

            if (user is null)
            {
                logger.LogWarning("Admin attempted to delete non-existing user | UserId={UserId}", id);

                throw new NotFoundException("User not found.");
            }

            await adminUserRepository.DeleteAsync(user);

            logger.LogInformation("Admin deleted user | UserId={UserId} | Email={Email}",user.Id,user.Email.Value);
        }
    }
}

