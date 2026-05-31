using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface ISourceTypeRepository
  {
    Task<List<SourceType>> GetAllAsync();
  }
}
