using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class SourceTypeRepository : ISourceTypeRepository
  {
    private readonly ApplicationDbContext _context;

    public SourceTypeRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<SourceType>> GetAllAsync()
    {
      return await _context.SourceTypes.ToListAsync();
    }
  }
}
