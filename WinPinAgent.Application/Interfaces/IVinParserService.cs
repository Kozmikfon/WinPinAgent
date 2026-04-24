using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPinAgent.Application.Interfaces
{
    public interface IVinParserService
    {
        string? Parse(string vin);
        bool IsValid(string vin);
    }
}
