using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class ContentTypeFieldRepository : IContentTypeFieldRepository
  {
    private readonly ApplicationDbContext _context;

    public ContentTypeFieldRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<ContentTypeField>> GetAllWithContentTypeAsync()
    {
      return await _context.ContentTypeFields
        .Include(c => c.ContentType)
        .ToListAsync();
    }

    public async Task<List<ContentTypeField>> GetAllAsync()
    {
      return await _context.ContentTypeFields.ToListAsync();
    }

    public async Task<List<ContentTypeField>> GetByContentTypeIdAsync(int contentTypeId)
    {
      return await _context.ContentTypeFields
        .Where(c => c.ContentTypeId == contentTypeId)
        .ToListAsync();
    }

    public async Task<ContentTypeField?> GetByIdAsync(int id)
    {
      return await _context.ContentTypeFields.SingleOrDefaultAsync(c => c.ContentTypeFieldId == id);
    }

    public async Task<ContentTypeField?> GetByIdWithContentTypeAsync(int id)
    {
      return await _context.ContentTypeFields
        .Include(c => c.ContentType)
        .Where(c => c.ContentTypeFieldId == id)
        .FirstOrDefaultAsync();
    }

    public async Task AddAsync(ContentTypeField contentTypeField)
    {
      _context.Add(contentTypeField);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ContentTypeField contentTypeField)
    {
      _context.Update(contentTypeField);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ContentTypeField contentTypeField)
    {
      _context.ContentTypeFields.Remove(contentTypeField);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
      return await _context.ContentTypeFields.AnyAsync(c => c.ContentTypeFieldId == id);
    }
  }
}
