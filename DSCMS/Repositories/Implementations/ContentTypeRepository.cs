using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class ContentTypeRepository : IContentTypeRepository
  {
    private readonly ApplicationDbContext _context;

    public ContentTypeRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<ContentType>> GetAllWithTemplateAsync()
    {
      return await _context.ContentTypes
        .Include(ct => ct.MultipleContentsTemplate)
        .ToListAsync();
    }

    public async Task<List<ContentType>> GetAllAsync()
    {
      return await _context.ContentTypes.ToListAsync();
    }

    public async Task<ContentType?> GetByIdAsync(int id)
    {
      return await _context.ContentTypes.SingleOrDefaultAsync(ct => ct.ContentTypeId == id);
    }

    public async Task<ContentType?> GetByIdWithTemplateAsync(int id)
    {
      return await _context.ContentTypes
        .Include(ct => ct.MultipleContentsTemplate)
        .SingleOrDefaultAsync(ct => ct.ContentTypeId == id);
    }

    public async Task<ContentType?> GetByIdWithFieldsAsync(int id)
    {
      return await _context.ContentTypes
        .Include(ct => ct.ContentTypeFields)
        .SingleOrDefaultAsync(ct => ct.ContentTypeId == id);
    }

    public async Task<ContentType?> GetByNameAsync(string name)
    {
      return await _context.ContentTypes
        .Include(ct => ct.ContentTypeFields)
        .Where(ct => ct.Name.ToLower() == name.ToLower())
        .FirstOrDefaultAsync();
    }

    public async Task AddAsync(ContentType contentType)
    {
      _context.Add(contentType);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ContentType contentType)
    {
      _context.Update(contentType);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ContentType contentType)
    {
      _context.ContentTypes.Remove(contentType);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
      return await _context.ContentTypes.AnyAsync(ct => ct.ContentTypeId == id);
    }
  }
}
