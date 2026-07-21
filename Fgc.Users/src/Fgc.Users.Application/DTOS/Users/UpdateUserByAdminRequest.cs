namespace Fgc.Users.Application.DTOS.Users;

public record UpdateUserByAdminRequest(
    string Name,
    string Email,
    string Role
);

