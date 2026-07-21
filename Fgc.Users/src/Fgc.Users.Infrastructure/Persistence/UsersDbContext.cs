using Fgc.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fgc.Users.Infrastructure.Persistence;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}