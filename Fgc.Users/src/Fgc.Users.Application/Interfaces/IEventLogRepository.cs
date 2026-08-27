namespace Fgc.Users.Application.Interfaces
{
    public interface IEventLogRepository
    {
        Task LogAsync(string eventType, object payload, CancellationToken cancellationToken);
    }
}
