using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailWithRoleAsync(string email); 
}
