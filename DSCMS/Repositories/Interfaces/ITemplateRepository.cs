using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface ITemplateRepository
  {
    Task<List<Template>> GetAllWithLayoutAsync();
    Task<List<Template>> GetAllAsync();
    Task<List<Template>> GetByIsForMultipleContentsAsync(int isForMultipleContents);
    Task<Template?> GetByIdAsync(int id);
    Task<Template?> GetByIdWithLayoutAsync(int id);
    Task AddAsync(Template template);
    Task UpdateAsync(Template template);
    Task DeleteAsync(Template template);
    Task<bool> ExistsAsync(int id);
  }
}
