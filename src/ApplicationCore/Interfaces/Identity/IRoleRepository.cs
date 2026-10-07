using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces;

public interface IRoleRepository: IRepository<Role>
{
    Task<Role?> GetRoleByNameAsync(string name);
}
