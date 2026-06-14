using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Repositories.Implementations
{
  public class UserRepository : IUserRepository
  {
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<ApplicationUser>> GetAllAsync()
    {
      return await _context.Users.ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
      return await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
      return await _context.Users.AnyAsync(u => u.Id == id);
    }
  }
}
