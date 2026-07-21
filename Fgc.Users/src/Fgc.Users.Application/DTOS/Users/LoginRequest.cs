namespace Fgc.Users.Application.DTOS.Users;

public record LoginRequest(
    string Email,
    string Password
);

