using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface IContentTypeRepository
  {
    Task<List<ContentType>> GetAllWithTemplateAsync();
    Task<List<ContentType>> GetAllAsync();
    Task<ContentType?> GetByIdAsync(int id);
    Task<ContentType?> GetByIdWithTemplateAsync(int id);
    Task<ContentType?> GetByIdWithFieldsAsync(int id);
    Task<ContentType?> GetByNameAsync(string name);
    Task AddAsync(ContentType contentType);
    Task UpdateAsync(ContentType contentType);
    Task DeleteAsync(ContentType contentType);
    Task<bool> ExistsAsync(int id);
  }
}
