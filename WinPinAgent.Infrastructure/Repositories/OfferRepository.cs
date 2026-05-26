using Microsoft.EntityFrameworkCore;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Domain.Interfaces;
using WinPinAgent.Infrastructure.Data;

namespace WinPinAgent.Infrastructure.Repositories;

public class OfferRepository : IOfferRepository
{
    private readonly AppDbContext _context;

    public OfferRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Offer offer)
    {
        await _context.Offers.AddAsync(offer);
        await _context.SaveChangesAsync();
    }

    public async Task<Offer?> GetByIdAsync(Guid id)
    => await _context.Offers
        .Include(o => o.PartRequest)
        .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<Offer>> GetByRequestIdAsync(Guid requestId)
        => await _context.Offers
            .Include(o => o.Seller)
            .Where(o => o.PartRequestId == requestId)
            .ToListAsync();
}