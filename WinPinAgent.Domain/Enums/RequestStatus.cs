using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPinAgent.Domain.Enums
{
    public enum RequestStatus
    {
        Pending,
        Broadcasted,
        OfferReceived,
        Accepted,
        Closed,
        Expired
    }
}
