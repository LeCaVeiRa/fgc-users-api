using Fgc.Users.Application.Helpers;
using Fgc.Users.Application.Interfaces;
using Fgc.Users.Domain.Entities;
using Fgc.Users.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fgc.Users.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
           _logger.LogWarning("Authentication failed | Reason=InvalidCredentials | Email={Email}", email);

             throw new ArgumentException("Invalid credentials.");
        }

        if (!PasswordHasher.Verify(password, user.PasswordHash))
        {
           _logger.LogWarning("Authentication failed | Reason=InvalidCredentials | UserId={UserId}", user.Id);
    
                throw new UnauthorizedException("Invalid credentials.");
        }

           _logger.LogInformation("User authenticated successfully | UserId={UserId}", user.Id);

        return user;
    }
}
