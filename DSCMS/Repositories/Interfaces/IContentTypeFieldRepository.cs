using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface IContentTypeFieldRepository
  {
    Task<List<ContentTypeField>> GetAllWithContentTypeAsync();
    Task<List<ContentTypeField>> GetAllAsync();
    Task<List<ContentTypeField>> GetByContentTypeIdAsync(int contentTypeId);
    Task<ContentTypeField?> GetByIdAsync(int id);
    Task<ContentTypeField?> GetByIdWithContentTypeAsync(int id);
    Task AddAsync(ContentTypeField contentTypeField);
    Task UpdateAsync(ContentTypeField contentTypeField);
    Task DeleteAsync(ContentTypeField contentTypeField);
    Task<bool> ExistsAsync(int id);
  }
}
