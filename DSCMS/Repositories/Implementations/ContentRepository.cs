using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class ContentRepository : IContentRepository
  {
    private readonly ApplicationDbContext _context;

    public ContentRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<Content>> GetAllWithDetailsAsync(string? contentTypeFilter = null)
    {
      var query = _context.Contents
        .Include(c => c.ContentType)
        .Include(c => c.CreatedByUser)
        .Include(c => c.LastUpdatedByUser)
        .Include(c => c.Template)
        .Include(c => c.ContentTypeFieldItems)
        .AsQueryable();

      if (!string.IsNullOrEmpty(contentTypeFilter))
        query = query.Where(c => c.ContentType.Name == contentTypeFilter);

      return await query.ToListAsync();
    }

    public async Task<List<Content>> GetAllSimpleAsync()
    {
      return await _context.Contents.ToListAsync();
    }

    public async Task<List<Content>> GetByContentTypeIdWithDetailsAsync(int contentTypeId)
    {
      return await _context.Contents
        .Where(c => c.ContentTypeId == contentTypeId)
        .Include(c => c.CreatedByUser)
        .Include(c => c.LastUpdatedByUser)
        .Include(c => c.ContentTypeFieldItems)
        .ThenInclude(ci => ci.ContentTypeField)
        .ToListAsync();
    }

    public async Task<Content?> GetByIdAsync(int id)
    {
      return await _context.Contents.SingleOrDefaultAsync(c => c.ContentId == id);
    }

    public async Task<Content?> GetByIdWithFieldItemsAsync(int id)
    {
      return await _context.Contents
        .Include(c => c.ContentTypeFieldItems)
        .ThenInclude(ci => ci.ContentTypeField)
        .SingleOrDefaultAsync(c => c.ContentId == id);
    }

    public async Task<Content?> GetByIdWithContentTypeAsync(int id)
    {
      return await _context.Contents
        .Include(c => c.ContentType)
        .Where(c => c.ContentId == id)
        .FirstOrDefaultAsync();
    }

    public async Task<Content?> GetByUrlAndContentTypeAsync(string url, int contentTypeId)
    {
      return await _context.Contents
        .Include(c => c.CreatedByUser)
        .Include(c => c.LastUpdatedByUser)
        .Include(c => c.ContentTypeFieldItems)
        .ThenInclude(ci => ci.ContentTypeField)
        .Where(c => c.UrlToDisplay == url && c.ContentTypeId == contentTypeId)
        .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Content content)
    {
      _context.Add(content);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Content content)
    {
      _context.Update(content);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Content content)
    {
      _context.Contents.Remove(content);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
      return await _context.Contents.AnyAsync(c => c.ContentId == id);
    }
  }
}
