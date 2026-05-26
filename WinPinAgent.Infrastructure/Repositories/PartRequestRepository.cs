using Microsoft.EntityFrameworkCore;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Domain.Enums;
using WinPinAgent.Domain.Interfaces;
using WinPinAgent.Infrastructure.Data;

namespace WinPinAgent.Infrastructure.Repositories;

public class PartRequestRepository : IPartRequestRepository
{
    private readonly AppDbContext _context;

    public PartRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PartRequest?> GetByIdAsync(Guid id)
        => await _context.PartRequests
            .Include(r => r.Buyer)
            .Include(r => r.Offers)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task AddAsync(PartRequest request)
    {
        await _context.PartRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PartRequest request)
    {
        _context.PartRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PartRequest>> GetExpiredRequestsAsync()
    => await _context.PartRequests
        .Where(r => r.ExpiresAt < DateTime.UtcNow
            && r.Status != RequestStatus.Accepted
            && r.Status != RequestStatus.Closed
            && r.Status != RequestStatus.Expired)
        .ToListAsync();

    public async Task<int> GetTotalCountAsync()
     => await _context.PartRequests.CountAsync();

    public async Task<int> GetCountByStatusAsync(RequestStatus status)
      => await _context.PartRequests
          .CountAsync(r => r.Status == status);

    public async Task<Dictionary<string, int>> GetTopBrandsAsync(int top = 5)
    {
        var brands = await _context.PartRequests
            .Select(r => r.Brand)
            .ToListAsync();

        return brands
            .GroupBy(b => b)
            .OrderByDescending(g => g.Count())
            .Take(top)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<double> GetAverageResponseTimeInMinutesAsync()
    {
        var accepted = await _context.PartRequests
            .Where(r => r.Status == RequestStatus.Accepted && r.AcceptedAt.HasValue)
            .Select(r => new { r.CreatedAt, r.AcceptedAt })
            .ToListAsync();

        if (!accepted.Any()) return 0;

        return accepted
            .Average(r => (r.AcceptedAt!.Value - r.CreatedAt).TotalMinutes);
    }

}