using Fgc.Users.Domain.Entities;

namespace Fgc.Users.Application.DTOS.Users;

public class UserResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }

    public static UserResponse FromEntity(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email.Value,
        Role = user.Role
    };
}
