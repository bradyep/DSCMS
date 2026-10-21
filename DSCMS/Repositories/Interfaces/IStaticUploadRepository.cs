using DSCMS.Models;

namespace DSCMS.Repositories.Interfaces
{
  public interface IStaticUploadRepository
  {
    Task<List<StaticUpload>> GetAllWithUploaderAsync();
    Task<StaticUpload?> GetByIdAsync(int id);
    Task<StaticUpload?> GetByIdWithFilesAsync(int id);
    Task<List<string>> GetAllFolderPathsAsync();
    Task AddAsync(StaticUpload upload);
    Task UpdateAsync(StaticUpload upload);
    Task ReplaceFilesAsync(StaticUpload upload, List<StaticUploadFile> files);
    Task DeleteAsync(StaticUpload upload);
  }
}
