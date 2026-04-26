using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPinAgent.Application.Interfaces
{
    public interface ITelegramNotificationService
    {
        Task SendMessageAsync(long chatId, string message);


    }
}
