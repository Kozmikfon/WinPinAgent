using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Enums;

namespace WinPinAgent.Domain.Entities
{
    public class PartRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Vin { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);

        public long BuyerId { get; set; }
        public User Buyer { get; set; } = null!;
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}
