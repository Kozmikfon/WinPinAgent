using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Application.Interfaces;

namespace WinPinAgent.Application.Services
{
    public class VinParserService : IVinParserService
    {
        // WMI (World Manufacturer Identifier) → İlk 3 karakter
        private static readonly Dictionary<string, string> WmiMap = new()
    {
        // Alman
        { "WBA", "BMW" }, { "WBS", "BMW" }, { "WBY", "BMW" },
        { "WAU", "AUDI" }, { "WVW", "VOLKSWAGEN" }, { "WDD", "MERCEDES" },
        // Japon
        { "JHM", "HONDA" }, { "JT2", "TOYOTA" }, { "JN1", "NISSAN" },
        { "JS1", "SUZUKI" }, { "JM1", "MAZDA" },
        // Amerikan
        { "1HG", "HONDA_US" }, { "1G1", "CHEVROLET" }, { "1FA", "FORD" },
        // Türk / Genel Avrupa
        { "NM4", "RENAULT_TR" }, { "VF1", "RENAULT" }, { "ZFA", "FIAT" },
    };

        public bool IsValid(string vin)
        {
            if (string.IsNullOrWhiteSpace(vin)) return false;
            if (vin.Length != 17) return false;
            // VIN geçersiz karakterler: I, O, Q
            return vin.ToUpper().All(c => char.IsLetterOrDigit(c) && c != 'I' && c != 'O' && c != 'Q');
        }

        public string? Parse(string vin)
        {
            if (!IsValid(vin)) return null;
            var wmi = vin.Substring(0, 3).ToUpper();
            return WmiMap.TryGetValue(wmi, out var brand) ? brand : null;
        }
    }
}
