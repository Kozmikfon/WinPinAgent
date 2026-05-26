using Microsoft.EntityFrameworkCore;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Domain.Interfaces;
using WinPinAgent.Infrastructure.Data;

namespace WinPinAgent.Infrastructure.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;

    public RatingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Rating rating)
    {
        await _context.Ratings.AddAsync(rating);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsForOfferAsync(Guid offerId)
        => await _context.Ratings.AnyAsync(r => r.OfferId == offerId);

    public async Task<double> GetAverageForUserAsync(long userId)
    {
        var ratings = await _context.Ratings
            .Where(r => r.RatedUserId == userId)
            .Select(r => r.Score)
            .ToListAsync();

        return ratings.Any() ? ratings.Average() : 0;
    }

    public async Task<int> GetTotalForUserAsync(long userId)
        => await _context.Ratings.CountAsync(r => r.RatedUserId == userId);
}