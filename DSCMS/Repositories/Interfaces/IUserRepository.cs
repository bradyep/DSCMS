using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface IUserRepository
  {
    Task<List<ApplicationUser>> GetAllAsync();
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<bool> ExistsAsync(string id);
  }
}
