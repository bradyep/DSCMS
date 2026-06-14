using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class TemplateRepository : ITemplateRepository
  {
    private readonly ApplicationDbContext _context;

    public TemplateRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<Template>> GetAllWithLayoutAsync()
    {
      return await _context.Templates.Include(t => t.Layout).ToListAsync();
    }

    public async Task<List<Template>> GetAllAsync()
    {
      return await _context.Templates.ToListAsync();
    }

    public async Task<List<Template>> GetByIsForMultipleContentsAsync(int isForMultipleContents)
    {
      return await _context.Templates
        .Where(t => t.IsForMultipleContents == isForMultipleContents)
        .ToListAsync();
    }

    public async Task<Template?> GetByIdAsync(int id)
    {
      return await _context.Templates.SingleOrDefaultAsync(t => t.TemplateId == id);
    }

    public async Task<Template?> GetByIdWithLayoutAsync(int id)
    {
      return await _context.Templates
        .Include(t => t.Layout)
        .Where(t => t.TemplateId == id)
        .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Template template)
    {
      _context.Add(template);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Template template)
    {
      _context.Update(template);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Template template)
    {
      _context.Templates.Remove(template);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
      return await _context.Templates.AnyAsync(t => t.TemplateId == id);
    }
  }
}
