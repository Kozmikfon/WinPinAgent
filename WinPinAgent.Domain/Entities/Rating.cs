using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPinAgent.Domain.Entities
{
    public class Rating
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Score { get; set; }        // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public long RaterId { get; set; }     // Puanlayan (alıcı)
        public User Rater { get; set; } = null!;

        public long RatedUserId { get; set; } // Puanlanan (satıcı)
        public User RatedUser { get; set; } = null!;

        public Guid OfferId { get; set; }     // Hangi teklif için
        public Offer Offer { get; set; } = null!;
    }
}
