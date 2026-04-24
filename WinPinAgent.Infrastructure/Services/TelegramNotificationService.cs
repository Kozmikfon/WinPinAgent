using Telegram.Bot;
using WinPinAgent.Application.Interfaces;

namespace WinPinAgent.Infrastructure.Services;

public class TelegramNotificationService : ITelegramNotificationService
{
    private readonly ITelegramBotClient _botClient;

    public TelegramNotificationService(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        await _botClient.SendMessage(chatId, message);
    }
}