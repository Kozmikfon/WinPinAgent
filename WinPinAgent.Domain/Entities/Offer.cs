using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPinAgent.Domain.Entities
{
    public class Offer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Price { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid PartRequestId { get; set; }
        public PartRequest PartRequest { get; set; } = null!;

        public long SellerId { get; set; }
        public User Seller { get; set; } = null!;
        public Rating? Rating { get; set; }
    }
}
