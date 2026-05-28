using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Domain.Enums;

namespace WinPinAgent.Domain.Interfaces
{
    public interface IPartRequestRepository
    {
        Task<PartRequest?> GetByIdAsync(Guid id);
        Task AddAsync(PartRequest request);
        Task UpdateAsync(PartRequest request);
        Task<IEnumerable<PartRequest>> GetExpiredRequestsAsync();
        Task<int> GetTotalCountAsync();
        Task<int> GetCountByStatusAsync(RequestStatus status);
        Task<Dictionary<string, int>> GetTopBrandsAsync(int top = 5);
        Task<double> GetAverageResponseTimeInMinutesAsync();

        Task<IEnumerable<PartRequest>> GetActiveByBrandAsync(string brand);
        Task<IEnumerable<PartRequest>> GetByBuyerIdAsync(long buyerId); 

    }
}
