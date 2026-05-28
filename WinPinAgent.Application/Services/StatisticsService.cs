using WinPinAgent.Application.Interfaces;
using WinPinAgent.Domain.Enums;
using WinPinAgent.Domain.Interfaces;

namespace WinPinAgent.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IPartRequestRepository _requestRepo;
    private readonly IUserRepository _userRepo;

    public StatisticsService(
        IPartRequestRepository requestRepo,
        IUserRepository userRepo)
    {
        _requestRepo = requestRepo;
        _userRepo = userRepo;
    }

    public async Task<string> GetDashboardTextAsync()
    {
        var total = await _requestRepo.GetTotalCountAsync();
        var pending = await _requestRepo.GetCountByStatusAsync(RequestStatus.Pending);
        var broadcasted = await _requestRepo.GetCountByStatusAsync(RequestStatus.Broadcasted);
        var offerReceived = await _requestRepo.GetCountByStatusAsync(RequestStatus.OfferReceived);
        var accepted = await _requestRepo.GetCountByStatusAsync(RequestStatus.Accepted);
        var expired = await _requestRepo.GetCountByStatusAsync(RequestStatus.Expired);
        var topBrands = await _requestRepo.GetTopBrandsAsync(5);
        var avgResponse = await _requestRepo.GetAverageResponseTimeInMinutesAsync();
        var totalUsers = await _userRepo.GetTotalCountAsync();
        var totalBuyers = await _userRepo.GetCountByRoleAsync(UserRole.Buyer);
        var totalSellers = await _userRepo.GetCountByRoleAsync(UserRole.Seller);
        var topSellers = await _userRepo.GetTopRatedSellersAsync(3);

        var brandsText = topBrands.Any()
            ? string.Join("\n", topBrands.Select((b, i) => $"   {i + 1}. {b.Key} — {b.Value} talep"))
            : "   Henüz veri yok";

        var avgText = avgResponse > 0
            ? $"{avgResponse:F1} dakika"
            : "Henüz veri yok";

        var sellersText = topSellers.Any()
            ? string.Join("\n", topSellers.Select((s, i) =>
                $"   {i + 1}. @{s.Username} — {s.AverageRating:F1}⭐ ({s.TotalRatings} değerlendirme)"))
            : "   Henüz veri yok";

        return
            $"📊 WinPin Dashboard\n" +
            $"{'─',25}\n\n" +
            $"👥 Kullanıcılar\n" +
            $"   Toplam: {totalUsers}\n" +
            $"   Alıcı: {totalBuyers} | Satıcı: {totalSellers}\n\n" +
            $"📋 Talepler\n" +
            $"   Toplam: {total}\n" +
            $"   ⏳ Bekleyen: {pending + broadcasted}\n" +
            $"   💬 Teklif Alan: {offerReceived}\n" +
            $"   ✅ Kabul Edilen: {accepted}\n" +
            $"   ⏰ Süresi Dolan: {expired}\n\n" +
            $"🏆 En Çok Aranan Markalar\n" +
            $"{brandsText}\n\n" +
            $"🏅 En Yüksek Puanlı Satıcılar\n" +
            $"{sellersText}\n\n" +
            $"⚡ Ort. Yanıt Süresi\n" +
            $"   {avgText}";
    }
}