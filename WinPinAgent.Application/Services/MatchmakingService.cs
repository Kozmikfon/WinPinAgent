
using WinPinAgent.Application.Interfaces;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Domain.Enums;
using WinPinAgent.Domain.Interfaces;

namespace WinPinAgent.Application.Services;

public class MatchmakingService : IMatchmakingService
{
    private readonly IPartRequestRepository _requestRepo;
    private readonly IUserRepository _userRepo;
    private readonly IOfferRepository _offerRepo;
    private readonly ITelegramNotificationService _notificationService;

    public MatchmakingService(
        IPartRequestRepository requestRepo,
        IUserRepository userRepo,
        IOfferRepository offerRepo,
        ITelegramNotificationService notificationService)
    {
        _requestRepo = requestRepo;
        _userRepo = userRepo;
        _offerRepo = offerRepo;
        _notificationService = notificationService;
    }

    public async Task AcceptOfferAsync(Guid partRequestId, Guid offerId)
    {
        var request = await _requestRepo.GetByIdAsync(partRequestId);
        if (request is null) return;
        if (request.Status == RequestStatus.Accepted ||
            request.Status == RequestStatus.Closed) return;

        // OfferReceived/Broadcasted → Accepted
        request.Status = RequestStatus.Accepted;
        request.AcceptedAt = DateTime.UtcNow;
        await _requestRepo.UpdateAsync(request);
    }

    public async Task BroadcastRequestAsync(Guid partRequestId)
    {
        var request = await _requestRepo.GetByIdAsync(partRequestId);
        if (request is null) return;
        if (request.Status != RequestStatus.Pending) return;

        var sellers = await _userRepo.GetSellersByBrandAsync(request.Brand);
        var sellerList = sellers.ToList();

        if (!sellerList.Any())
        {
            await _notificationService.SendMessageAsync(request.BuyerId,
                "⚠️ Şu an bu marka için kayıtlı tedarikçi bulunamadı. Talebiniz açık kalacak.");
            return;
        }

        var tasks = sellerList.Select(seller =>
            _notificationService.SendMessageAsync(
                seller.Id,
                $"🔔 Yeni Talep!\n" +
                $"Araç: {request.Brand}\n" +
                $"VIN: {request.Vin}\n" +
                $"Parça: {request.PartName}\n" +
                $"Son geçerlilik: {request.ExpiresAt:dd.MM.yyyy HH:mm}\n\n" +
                $"Teklif vermek için:\n/teklif {request.Id} [fiyat] [stok durumu]"
            )
        );

        await Task.WhenAll(tasks);

        // Pending → Broadcasted
        request.Status = RequestStatus.Broadcasted;
        await _requestRepo.UpdateAsync(request);
    }

    public async Task ExpireRequestsAsync()
    {
        var expiredRequests = await _requestRepo.GetExpiredRequestsAsync();
        foreach (var request in expiredRequests)
        {
            request.Status = RequestStatus.Expired;
            await _requestRepo.UpdateAsync(request);

            await _notificationService.SendMessageAsync(request.BuyerId,
                $"⏰ Talebiniz süresi dolduğu için kapatıldı.\n" +
                $"Parça: {request.PartName}\n" +
                $"Yeni talep oluşturmak için /talep komutunu kullanabilirsiniz.");
        }
    }
}