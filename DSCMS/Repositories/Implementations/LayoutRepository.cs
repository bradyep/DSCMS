using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class LayoutRepository : ILayoutRepository
  {
    private readonly ApplicationDbContext _context;

    public LayoutRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<Layout>> GetAllAsync()
    {
      return await _context.Layouts.ToListAsync();
    }

    public async Task<Layout?> GetByIdAsync(int id)
    {
      return await _context.Layouts.FirstOrDefaultAsync(l => l.LayoutId == id);
    }

    public async Task<int> CountAsync()
    {
      return await _context.Layouts.CountAsync();
    }

    public async Task AddAsync(Layout layout)
    {
      _context.Layouts.Add(layout);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Layout layout)
    {
      _context.Update(layout);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Layout layout)
    {
      _context.Layouts.Remove(layout);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
      return await _context.Layouts.AnyAsync(l => l.LayoutId == id);
    }
  }
}
