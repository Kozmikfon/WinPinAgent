using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Enums;

namespace WinPinAgent.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }           // Telegram Chat ID
        public string Username { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public List<string> BrandExpertise { get; set; } = new(); // ["BMW", "AUDI"]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PartRequest> Requests { get; set; } = new List<PartRequest>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();
        public ICollection<Rating> ReceivedRatings { get; set; } = new List<Rating>();
        public double AverageRating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
    }
}
