using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Entities;

namespace WinPinAgent.Domain.Interfaces
{
    public interface IOfferRepository
    {
        Task AddAsync(Offer offer);
        Task<IEnumerable<Offer>> GetByRequestIdAsync(Guid requestId);
    }
}
