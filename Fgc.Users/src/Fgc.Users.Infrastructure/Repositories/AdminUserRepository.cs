using Fgc.Users.Application.Interfaces;
using Fgc.Users.Domain.Entities;
using Fgc.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fgc.Users.Infrastructure.Repositories
{
    public class AdminUserRepository: IAdminUserRepository
    {
        private readonly UsersDbContext _context;

        public AdminUserRepository(UsersDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == email);
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

    }
}
