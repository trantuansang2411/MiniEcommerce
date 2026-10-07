using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserRepository
    : EfRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }
    public async Task<Role?> GetRoleAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(x => x.Name == name);
    }
    public async Task<User?> GetByEmailWithRoleAsync(string email)
    {
        return await _context.Users.Include(x=>x.Role)
            .FirstOrDefaultAsync(x => x.Email == email);
    }
}