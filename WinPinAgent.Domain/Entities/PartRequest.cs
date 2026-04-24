using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPinAgent.Domain.Entities
{
    public class PartRequest
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Vin { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;   // VIN'den parse edilir
        public string PartName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";      // Pending, Matched, Closed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public long BuyerId { get; set; }
        public User Buyer { get; set; } = null!;
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}
