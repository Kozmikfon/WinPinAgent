using WinPinAgent.Application.Interfaces;

namespace WinPinAgent.API.BackgroundServices;

public class RequestExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RequestExpiryService> _logger;

    public RequestExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<RequestExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var matchmaking = scope.ServiceProvider
                    .GetRequiredService<IMatchmakingService>();

                await matchmaking.ExpireRequestsAsync();
                _logger.LogInformation("Süresi dolan talepler kontrol edildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Expire servisi hatası.");
            }

            // Her 1 saatte bir çalış
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}