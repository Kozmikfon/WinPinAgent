using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Entities;

namespace WinPinAgent.Domain.Interfaces
{
    public interface IRatingRepository
    {
        Task AddAsync(Rating rating);
        Task<bool> ExistsForOfferAsync(Guid offerId);
        Task<double> GetAverageForUserAsync(long userId);
        Task<int> GetTotalForUserAsync(long userId);
    }
}
