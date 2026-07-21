namespace Fgc.Users.Application.DTOS.Users
{
    public record RegisterUserRequest(
        string Name,
        string Email,
        string Password
    );
}
