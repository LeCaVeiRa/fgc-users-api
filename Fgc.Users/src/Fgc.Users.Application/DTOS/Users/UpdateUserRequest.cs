namespace Fgc.Users.Application.DTOS.Users
{
    public class UpdateUserRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}

