using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Entities;

namespace WinPinAgent.Domain.Interfaces
{
    public interface IPartRequestRepository
    {
        Task<PartRequest?> GetByIdAsync(Guid id);
        Task AddAsync(PartRequest request);
        Task UpdateAsync(PartRequest request);
        Task<IEnumerable<PartRequest>> GetExpiredRequestsAsync();

    }
}
