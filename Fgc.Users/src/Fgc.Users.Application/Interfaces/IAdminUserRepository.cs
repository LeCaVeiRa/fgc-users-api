using Fgc.Users.Domain.Entities;

namespace Fgc.Users.Application.Interfaces
{
    public interface IAdminUserRepository
    {
        Task AddAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
       
    }
}

