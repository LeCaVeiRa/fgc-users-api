
using Fgc.Users.Application.Helpers;
using Fgc.Users.Domain.Entities;
using Fgc.Users.Domain.ValueObjects;
using Fgc.Users.Infrastructure.Persistence;

namespace Fgc.Users.Infrastructure.Seed;

public static class AdminUserSeed
{
    public static async Task SeedAsync(UsersDbContext context)
    {
        if (context.Users.Any(u => u.Role == "Admin"))
            return;

        var admin = User.Create(
            name: "Admin",
            email: Email.Create("admin@Fgc.com"),
            passwordHash: PasswordHasher.Hash("Admin@123"),
            role: "Admin"
        );

        admin.UpdateRole("Admin");

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}

