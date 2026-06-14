using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface IContentTypeFieldItemRepository
  {
    Task<List<ContentTypeFieldItem>> GetAllWithDetailsAsync();
    Task<ContentTypeFieldItem?> GetByIdAsync(int id);
    Task<ContentTypeFieldItem?> GetByIdWithContentAsync(int id);
    Task AddAsync(ContentTypeFieldItem item);
    Task UpdateAsync(ContentTypeFieldItem item);
    Task DeleteAsync(ContentTypeFieldItem item);
    Task<bool> ExistsAsync(int id);
  }
}
