using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Entities;

namespace WinPinAgent.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(long telegramId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<IEnumerable<User>> GetSellersByBrandAsync(string brand);
    }
}
