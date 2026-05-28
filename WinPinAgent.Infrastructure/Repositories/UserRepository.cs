using Microsoft.EntityFrameworkCore;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Domain.Enums;
using WinPinAgent.Domain.Interfaces;
using WinPinAgent.Infrastructure.Data;

namespace WinPinAgent.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(long telegramId)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == telegramId);

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetSellersByBrandAsync(string brand)
    => await _context.Users
        .Where(u => u.BrandExpertise.Contains(brand))
        .ToListAsync();

    public async Task<int> GetTotalCountAsync()
     => await _context.Users.CountAsync();

    public async Task<int> GetCountByRoleAsync(UserRole role)
    => await _context.Users.CountAsync(u => u.Role == role);

    public async Task<IEnumerable<User>> GetTopRatedSellersAsync(int top = 3)
    => await _context.Users
        .Where(u => u.TotalRatings > 0)
        .OrderByDescending(u => u.AverageRating)
        .Take(top)
        .ToListAsync();
}