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

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            var update = JsonSerializer.Deserialize<Update>(body, options);
            if (update is null) return Ok();

            await _handler.HandleAsync(update);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hata: {Message}", ex.Message);
            return Ok();
        }
    }
}