using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface IContentRepository
  {
    Task<List<Content>> GetAllWithDetailsAsync(string? contentTypeFilter = null);
    Task<List<Content>> GetAllSimpleAsync();
    Task<List<Content>> GetByContentTypeIdWithDetailsAsync(int contentTypeId);
    Task<Content?> GetByIdAsync(int id);
    Task<Content?> GetByIdWithFieldItemsAsync(int id);
    Task<Content?> GetByIdWithContentTypeAsync(int id);
    Task<Content?> GetByUrlAndContentTypeAsync(string url, int contentTypeId);
    Task AddAsync(Content content);
    Task UpdateAsync(Content content);
    Task DeleteAsync(Content content);
    Task<bool> ExistsAsync(int id);
  }
}
