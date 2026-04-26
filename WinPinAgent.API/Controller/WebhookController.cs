using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Telegram.Bot.Types;
using WinPinAgent.API.Bot;

namespace WinPinAgent.API.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly BotUpdateHandler _handler;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(BotUpdateHandler handler, ILogger<WebhookController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Callback query
            if (root.TryGetProperty("callback_query", out var cbEl))
            {
                var chatId = cbEl.GetProperty("from").GetProperty("id").GetInt64();
                var data = cbEl.TryGetProperty("data", out var dataEl) ? dataEl.GetString() ?? "" : "";
                var callbackId = cbEl.GetProperty("id").GetString() ?? "";

                _logger.LogInformation("Callback geldi: {Data}", data);
                await _handler.HandleCallbackAsync(chatId, data, callbackId);
                return Ok();
            }

            // Normal mesaj
            if (root.TryGetProperty("message", out var msgEl))
            {
                var chatId = msgEl.GetProperty("chat").GetProperty("id").GetInt64();
                var text = msgEl.TryGetProperty("text", out var textEl) ? textEl.GetString() ?? "" : "";
                var username = msgEl.GetProperty("from").TryGetProperty("username", out var unEl)
                    ? unEl.GetString() : null;
                var firstName = msgEl.GetProperty("from").GetProperty("first_name").GetString() ?? "";

                _logger.LogInformation("Mesaj geldi: {Text}", text);
                await _handler.HandleMessageAsync(chatId, text, username, firstName);
                return Ok();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hata: {Message}", ex.Message);
            return Ok();
        }
    }
}