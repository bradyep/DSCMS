using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class ContentTypeFieldItemRepository : IContentTypeFieldItemRepository
  {
    private readonly ApplicationDbContext _context;

    public ContentTypeFieldItemRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<ContentTypeFieldItem>> GetAllWithDetailsAsync()
    {
      return await _context.ContentTypeFieldItems
        .Include(c => c.Content)
        .Include(c => c.ContentTypeField)
        .ToListAsync();
    }

    public async Task<ContentTypeFieldItem?> GetByIdAsync(int id)
    {
      return await _context.ContentTypeFieldItems.SingleOrDefaultAsync(c => c.ContentTypeFieldItemId == id);
    }

    public async Task<ContentTypeFieldItem?> GetByIdWithContentAsync(int id)
    {
      return await _context.ContentTypeFieldItems
        .Include(c => c.Content)
        .Where(c => c.ContentTypeFieldItemId == id)
        .FirstOrDefaultAsync();
    }

    public async Task AddAsync(ContentTypeFieldItem item)
    {
      _context.Add(item);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ContentTypeFieldItem item)
    {
      _context.Update(item);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ContentTypeFieldItem item)
    {
      _context.ContentTypeFieldItems.Remove(item);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
      return await _context.ContentTypeFieldItems.AnyAsync(c => c.ContentTypeFieldItemId == id);
    }
  }
}
