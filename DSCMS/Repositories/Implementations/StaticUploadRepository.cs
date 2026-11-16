using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class StaticUploadRepository : IStaticUploadRepository
  {
    private readonly ApplicationDbContext _context;

    public StaticUploadRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<StaticUpload>> GetAllWithUploaderAsync()
    {
      return await _context.StaticUploads
        .Include(u => u.UploadedByUser)
        .OrderBy(u => u.FolderPath)
        .ToListAsync();
    }

    public async Task<StaticUpload?> GetByIdAsync(int id)
    {
      return await _context.StaticUploads.FirstOrDefaultAsync(u => u.StaticUploadId == id);
    }

    public async Task<StaticUpload?> GetByIdWithFilesAsync(int id)
    {
      return await _context.StaticUploads
        .Include(u => u.Files.OrderBy(f => f.RelativePath))
        .Include(u => u.UploadedByUser)
        .FirstOrDefaultAsync(u => u.StaticUploadId == id);
    }

    public async Task<List<string>> GetAllFolderPathsAsync()
    {
      return await _context.StaticUploads.Select(u => u.FolderPath).ToListAsync();
    }

    public async Task AddAsync(StaticUpload upload)
    {
      _context.StaticUploads.Add(upload);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(StaticUpload upload)
    {
      _context.Update(upload);
      await _context.SaveChangesAsync();
    }

    public async Task ReplaceFilesAsync(StaticUpload upload, List<StaticUploadFile> files)
    {
      _context.StaticUploadFiles.RemoveRange(_context.StaticUploadFiles.Where(f => f.StaticUploadId == upload.StaticUploadId));
      upload.Files = files;
      _context.Update(upload);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(StaticUpload upload)
    {
      _context.StaticUploads.Remove(upload);
      await _context.SaveChangesAsync();
    }
  }
}
