using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPinAgent.Application.Dtos
{
    public class CreateOfferDto
    {
        public Guid PartRequestId { get; set; }
        public long SellerId { get; set; }
        public decimal Price { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
