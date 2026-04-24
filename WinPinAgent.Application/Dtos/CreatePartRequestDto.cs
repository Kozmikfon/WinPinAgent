using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPinAgent.Application.Dtos
{
    public class CreatePartRequestDto
    {
        public string Vin { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public long BuyerId { get; set; }
    }
}
