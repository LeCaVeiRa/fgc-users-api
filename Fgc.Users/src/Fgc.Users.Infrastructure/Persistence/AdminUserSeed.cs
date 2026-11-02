using Fgc.Users.Application.Helpers;
using Fgc.Users.Domain.Entities;
using Fgc.Users.Domain.ValueObjects;
using Fgc.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Fgc.Users.Infrastructure.Seed;

// Cria o primeiro Admin fora de Development, onde POST /setup/first-admin não existe.
// Executado pelo Job de migração (Program --migrate) com Seed:AdminEmail/Seed:AdminPassword vindos de Secret.
// Idempotente: não faz nada se já houver um Admin ou se as variáveis não estiverem definidas.
public static class AdminUserSeed
{
    public static async Task SeedAsync(UsersDbContext context, IConfiguration configuration, ILogger logger)
    {
        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("Admin seed skipped: Seed:AdminEmail/Seed:AdminPassword not set");
            return;
        }

        if (await context.Users.AnyAsync(u => u.Role == "Admin"))
        {
            logger.LogInformation("Admin seed skipped: an admin already exists");
            return;
        }

        PasswordValidator.Validate(password);

        var admin = User.Create(
            name: configuration["Seed:AdminName"] is { Length: > 0 } name ? name : "Admin",
            email: Email.Create(email),
            passwordHash: PasswordHasher.Hash(password),
            role: "Admin"
        );

        context.Users.Add(admin);
        await context.SaveChangesAsync();
        logger.LogInformation("Admin seed created first admin | UserId={UserId}", admin.Id);
    }
}
