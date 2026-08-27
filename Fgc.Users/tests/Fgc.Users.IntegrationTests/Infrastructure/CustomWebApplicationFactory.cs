using Fgc.Users.Application.Interfaces;
using Fgc.Users.Application.Services;
using Fgc.Users.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<UsersDbContext>));
            services.RemoveAll(typeof(DbContextOptions));

            services.AddSingleton<ILogger>(provider =>
                provider.GetRequiredService<ILogger<AdminUserService>>());

            services.AddScoped<DbContextOptions<UsersDbContext>>(provider =>
            {
                return new DbContextOptionsBuilder<UsersDbContext>()
                    .UseInMemoryDatabase("InMemoryUsersTestDb")
                    .Options;
            });

            // Sem broker/DynamoDB real disponível em teste de integração.
            services.RemoveAll(typeof(IEventLogRepository));
            services.AddScoped<IEventLogRepository, NoOpEventLogRepository>();
        });
    }
}

internal class NoOpEventLogRepository : IEventLogRepository
{
    public Task LogAsync(string eventType, object payload, CancellationToken cancellationToken) => Task.CompletedTask;
}
