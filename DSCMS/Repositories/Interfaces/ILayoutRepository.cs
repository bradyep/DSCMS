using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface ILayoutRepository
  {
    Task<List<Layout>> GetAllAsync();
    Task<Layout?> GetByIdAsync(int id);
    Task<int> CountAsync();
    Task AddAsync(Layout layout);
    Task UpdateAsync(Layout layout);
    Task DeleteAsync(Layout layout);
    Task<bool> ExistsAsync(int id);
  }
}
