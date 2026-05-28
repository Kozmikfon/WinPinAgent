using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WinPinAgent.Application.Interfaces;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Domain.Interfaces;
using WinPinAgent.Domain.Enums;

using Telegram.Bot.Types.ReplyMarkups;

using DomainUser = WinPinAgent.Domain.Entities.User;


namespace WinPinAgent.API.Bot;

public class BotUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IVinParserService _vinParser;
    private readonly IMatchmakingService _matchmaking;
    private readonly IUserRepository _userRepo;
    private readonly IPartRequestRepository _requestRepo;
    private readonly  IOfferRepository _offerRepo;
    private readonly IStatisticsService _statisticsService;
    private readonly IRatingRepository _ratingRepo;

    public BotUpdateHandler(
        ITelegramBotClient botClient,
        IVinParserService vinParser,
        IMatchmakingService matchmaking,
        IUserRepository userRepo,
        IPartRequestRepository requestRepo,
        IOfferRepository offerRepo,
        IStatisticsService statisticsService,
        IRatingRepository ratingRepo)
    {
        _botClient = botClient;
        _vinParser = vinParser;
        _matchmaking = matchmaking;
        _userRepo = userRepo;
        _requestRepo = requestRepo;
        _offerRepo = offerRepo;
        _statisticsService = statisticsService;
        _ratingRepo = ratingRepo;
    }





    private async Task HandleRegisterBuyerAsync(long chatId, string? username)
    {
        var existing = await _userRepo.GetByIdAsync(chatId);
        if (existing is not null)
        {
            await _botClient.SendMessage(chatId, "Zaten kayıtlısınız.");
            return;
        }

        var user = new DomainUser
        {
            Id = chatId,
            Username = username ?? "bilinmiyor",
            Role = UserRole.Buyer
        };

        await _userRepo.AddAsync(user);
        await _botClient.SendMessage(chatId, "✅ Alıcı olarak kaydoldunuz.");
    }

    private async Task HandleRegisterSellerAsync(long chatId, string[] args, string? username)
    {
        if (args.Length < 2)
        {
            await _botClient.SendMessage(chatId, "Kullanım: /kayit_satici BMW AUDI HONDA");
            return;
        }

        var brands = args.Skip(1).Select(b => b.ToUpper()).ToList();

        var existing = await _userRepo.GetByIdAsync(chatId);
        if (existing is not null)
        {
            existing.BrandExpertise = brands;
            await _userRepo.UpdateAsync(existing);
            await _botClient.SendMessage(chatId, $"✅ Marka listesi güncellendi: {string.Join(", ", brands)}");
            return;
        }

        var user = new DomainUser
        {
            Id = chatId,
            Username = username ?? "bilinmiyor",
            Role = UserRole.Seller,
            BrandExpertise = brands
        };

        await _userRepo.AddAsync(user);
        await _botClient.SendMessage(chatId, $"✅ Satıcı olarak kaydoldunuz. Markalar: {string.Join(", ", brands)}");
    }

    private async Task HandlePartRequestAsync(long chatId, string[] args)
    {
        if (args.Length < 3)
        {
            await _botClient.SendMessage(chatId, "Kullanım: /talep WBA3A5G50DNP26546 ön_tampon");
            return;
        }

        var vin = args[1].ToUpper();
        var partName = string.Join(" ", args.Skip(2));

        if (!_vinParser.IsValid(vin))
        {
            await _botClient.SendMessage(chatId, "❌ Geçersiz VIN numarası. Lütfen 17 haneli şasi numaranızı kontrol edin.");
            return;
        }

        var brand = _vinParser.Parse(vin);
        if (brand is null)
        {
            await _botClient.SendMessage(chatId, "⚠️ Araç markası tespit edilemedi. Sistem bu markayı henüz tanımıyor.");
            return;
        }

        var request = new PartRequest
        {
            Vin = vin,
            Brand = brand,
            PartName = partName,
            BuyerId = chatId
        };

        await _requestRepo.AddAsync(request);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
    new[]
    {
        InlineKeyboardButton.WithCallbackData(
            "📋 Teklifleri Gör",
            $"teklifler:{request.Id}")
    }
});

        await _botClient.SendMessage(chatId,
            $"✅ Talebiniz oluşturuldu!\n" +
            $"Araç: {brand}\n" +
            $"Parça: {partName}\n" +
            $"Talep No: {request.Id}\n\n" +
            $"İlgili tedarikçiler bilgilendiriliyor...",
            replyMarkup: keyboard);

        await _matchmaking.BroadcastRequestAsync(request.Id);
    }

    private async Task HandleOfferAsync(long chatId, string[] args)
    {
        if (args.Length < 4)
        {
            await _botClient.SendMessage(chatId, "Kullanım: /teklif [TALEP_ID] [FİYAT] [STOK_DURUMU]");
            return;
        }

        if (!Guid.TryParse(args[1], out var requestId))
        {
            await _botClient.SendMessage(chatId, "❌ Geçersiz talep ID.");
            return;
        }

        if (!decimal.TryParse(args[2], out var price))
        {
            await _botClient.SendMessage(chatId, "❌ Geçersiz fiyat formatı.");
            return;
        }

        var request = await _requestRepo.GetByIdAsync(requestId);
        if (request is null)
        {
            await _botClient.SendMessage(chatId, "❌ Talep bulunamadı.");
            return;
        }

        if (request.Status == Domain.Enums.RequestStatus.Accepted ||
            request.Status == Domain.Enums.RequestStatus.Closed ||
            request.Status == Domain.Enums.RequestStatus.Expired)
        {
            await _botClient.SendMessage(chatId, "⚠️ Bu talep artık aktif değil.");
            return;
        }

        var stockStatus = string.Join(" ", args.Skip(3));

        var offer = new Offer
        {
            PartRequestId = requestId,
            SellerId = chatId,
            Price = price,
            StockStatus = stockStatus
        };

        // Teklifi veritabanına kaydet
        await _offerRepo.AddAsync(offer);

        // Status güncelle: OfferReceived
        request.Status = Domain.Enums.RequestStatus.OfferReceived;
        await _requestRepo.UpdateAsync(request);

        await _botClient.SendMessage(chatId, "✅ Teklifiniz iletildi.");
        await _botClient.SendMessage(request.BuyerId,
            $"💬 Yeni teklif geldi!\n" +
            $"Parça: {request.PartName}\n" +
            $"Fiyat: {price:C}\n" +
            $"Stok: {stockStatus}\n\n" +
            $"Teklifleri görmek için: /teklifler {requestId}");
    }

    private async Task HandleListOffersAsync(long chatId, string[] args)
    {
        if (args.Length < 2)
        {
            await _botClient.SendMessage(chatId, "Kullanım: /teklifler [TALEP_ID]");
            return;
        }

        if (!Guid.TryParse(args[1], out var requestId))
        {
            await _botClient.SendMessage(chatId, "❌ Geçersiz talep ID.");
            return;
        }

        var request = await _requestRepo.GetByIdAsync(requestId);
        if (request is null)
        {
            await _botClient.SendMessage(chatId, "❌ Talep bulunamadı.");
            return;
        }

        //if (request.BuyerId != chatId)
        //{
        //    await _botClient.SendMessage(chatId, "❌ Bu talep size ait değil.");
        //    return;
        //}

        if (!request.Offers.Any())
        {
            await _botClient.SendMessage(chatId,
                $"📭 Henüz teklif gelmedi.\n" +
                $"Durum: {request.Status}\n" +
                $"Son geçerlilik: {request.ExpiresAt:dd.MM.yyyy HH:mm}");
            return;
        }

        await _botClient.SendMessage(chatId,
            $"📋 Teklifler — {request.PartName} ({request.Brand})\n" +
            $"Durum: {request.Status} | {request.Offers.Count()} teklif");

        int i = 1;
        foreach (var offer in request.Offers)
        {
            var offerKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"✅ Kabul Et",
                        $"k:{offer.Id}"),
                    InlineKeyboardButton.WithCallbackData(
                        $"❌ Reddet",
                        $"r:{offer.Id}")
                }
            });

            await _botClient.SendMessage(chatId,
                $"{i}. Teklif\n" +
                $"Fiyat: {offer.Price:C}\n" +
                $"Stok: {offer.StockStatus}",
                replyMarkup: offerKeyboard);
            i++;
        }
    }


    private async Task HandleAcceptOfferAsync(long chatId, string[] args)
    {
        if (args.Length < 3)
        {
            await _botClient.SendMessage(chatId, "Kullanım: /kabul [TALEP_ID] [TEKLIF_ID]");
            return;
        }

        if (!Guid.TryParse(args[1], out var requestId))
        {
            await _botClient.SendMessage(chatId, "❌ Geçersiz talep ID.");
            return;
        }

        if (!Guid.TryParse(args[2], out var offerId))
        {
            await _botClient.SendMessage(chatId, "❌ Geçersiz teklif ID.");
            return;
        }

        var request = await _requestRepo.GetByIdAsync(requestId);
        if (request is null)
        {
            await _botClient.SendMessage(chatId, "❌ Talep bulunamadı.");
            return;
        }

        if (request.BuyerId != chatId)
        {
            await _botClient.SendMessage(chatId, "❌ Bu talep size ait değil.");
            return;
        }

        if (request.Status == Domain.Enums.RequestStatus.Accepted)
        {
            await _botClient.SendMessage(chatId, "⚠️ Bu talep zaten kabul edildi.");
            return;
        }

        var offer = request.Offers.FirstOrDefault(o => o.Id == offerId);
        if (offer is null)
        {
            await _botClient.SendMessage(chatId, "❌ Teklif bulunamadı.");
            return;
        }

        await _matchmaking.AcceptOfferAsync(requestId, offerId);

        // Alıcıya bildir
        await _botClient.SendMessage(chatId,
            $"✅ Teklif kabul edildi!\n" +
            $"Parça: {request.PartName}\n" +
            $"Fiyat: {offer.Price:C}\n" +
            $"Stok: {offer.StockStatus}\n\n" +
            $"Tedarikçi ile iletişime geçebilirsiniz.");

        // Satıcıya bildir
        await _botClient.SendMessage(offer.SellerId,
            $"🎉 Teklifiniz kabul edildi!\n" +
            $"Parça: {request.PartName}\n" +
            $"Fiyat: {offer.Price:C}\n\n" +
            $"Alıcı ile iletişime geçin.");

        // Puanlama butonları
        var ratingKeyboard = new InlineKeyboardMarkup(new[]
        {
    new[]
    {
        InlineKeyboardButton.WithCallbackData("⭐ 1", $"puan:1:{offer.Id}"),
        InlineKeyboardButton.WithCallbackData("⭐ 2", $"puan:2:{offer.Id}"),
        InlineKeyboardButton.WithCallbackData("⭐ 3", $"puan:3:{offer.Id}"),
        InlineKeyboardButton.WithCallbackData("⭐ 4", $"puan:4:{offer.Id}"),
        InlineKeyboardButton.WithCallbackData("⭐ 5", $"puan:5:{offer.Id}")
    }
});

        await _botClient.SendMessage(chatId,
            $"🌟 Tedarikçiyi puanlamak ister misiniz?",
            replyMarkup: ratingKeyboard);

    }

    

    public async Task HandleMessageAsync(long chatId, string text, string? username, string firstName)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var args = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = args[0].ToLower();

        switch (command)
        {
            case "/start":
                await _botClient.SendMessage(chatId,
                    "👋 WinPin'e hoş geldiniz!\n\n" +
                    "Alıcıysanız: /kayit_alici\n" +
                    "Satıcıysanız: /kayit_satici BMW AUDI");
                break;
            case "/kayit_alici":
                await HandleRegisterBuyerAsync(chatId, username);
                break;
            case "/kayit_satici":
                await HandleRegisterSellerAsync(chatId, args, username);
                break;
            case "/talep":
                await HandlePartRequestAsync(chatId, args);
                break;
            case "/teklif":
                await HandleOfferAsync(chatId, args);
                break;
            case "/teklifler":
                await HandleListOffersAsync(chatId, args);
                break;
            case "/kabul":
                await HandleAcceptOfferAsync(chatId, args);
                break;
            case "/istatistik":
                var dashboard = await _statisticsService.GetDashboardTextAsync();
                await _botClient.SendMessage(chatId, dashboard);
                break;
            case "/profil":
                await HandleProfileAsync(chatId);
                break;

            default:
                await _botClient.SendMessage(chatId,
                    "Geçersiz komut. Kullanılabilir komutlar:\n" +
                    "/kayit_alici — Alıcı olarak kaydol\n" +
                    "/kayit_satici [MARKA1] [MARKA2] — Satıcı olarak kaydol\n" +
                    "/talep [VIN] [PARÇA_ADI] — Parça talebi oluştur\n" +
                    "/teklif [TALEP_ID] [FİYAT] [STOK] — Teklif ver");
                break;
        }
    }

    public async Task HandleCallbackAsync(long chatId, string data, string callbackId)
    {
        var parts = data.Split(':');

        switch (parts[0])
        {
            case "teklifler":
                await HandleListOffersAsync(chatId, new[] { "/teklifler", parts[1] });
                break;

            case "k":
                var offerId = Guid.Parse(parts[1]);
                var foundOffer = await _offerRepo.GetByIdAsync(offerId);
                if (foundOffer is null)
                {
                    await _botClient.SendMessage(chatId, "❌ Teklif bulunamadı.");
                    break;
                }
                await HandleAcceptOfferAsync(chatId,
                    new[] { "/kabul", foundOffer.PartRequestId.ToString(), foundOffer.Id.ToString() });
                break;

            case "r":
                await _botClient.SendMessage(chatId, "❌ Teklif reddedildi.");
                break;

            case "puan":
                var score = int.Parse(parts[1]);
                var ratedOfferId = Guid.Parse(parts[2]);

                var alreadyRated = await _ratingRepo.ExistsForOfferAsync(ratedOfferId);
                if (alreadyRated)
                {
                    await _botClient.SendMessage(chatId, "⚠️ Bu teklif için zaten puan verdiniz.");
                    break;
                }

                var ratedOffer = await _offerRepo.GetByIdAsync(ratedOfferId);
                if (ratedOffer is null)
                {
                    await _botClient.SendMessage(chatId, "❌ Teklif bulunamadı.");
                    break;
                }

                var rating = new WinPinAgent.Domain.Entities.Rating
                {
                    Score = score,
                    RaterId = chatId,
                    RatedUserId = ratedOffer.SellerId,
                    OfferId = ratedOfferId
                };

                await _ratingRepo.AddAsync(rating);

                // Satıcının ortalama puanını güncelle
                var seller = await _userRepo.GetByIdAsync(ratedOffer.SellerId);
                if (seller is not null)
                {
                    seller.AverageRating = await _ratingRepo.GetAverageForUserAsync(ratedOffer.SellerId);
                    seller.TotalRatings = await _ratingRepo.GetTotalForUserAsync(ratedOffer.SellerId);
                    await _userRepo.UpdateAsync(seller);
                }

                var stars = new string('⭐', score);
                await _botClient.SendMessage(chatId,
                    $"✅ Puanınız kaydedildi!\n{stars} ({score}/5)");

                await _botClient.SendMessage(ratedOffer.SellerId,
                    $"🌟 Yeni bir değerlendirme aldınız!\n" +
                    $"{stars} ({score}/5)");
                break;
        }

        try { await _botClient.AnswerCallbackQuery(callbackId); } catch { }
    }
    private async Task HandleProfileAsync(long chatId)
    {
        var user = await _userRepo.GetByIdAsync(chatId);
        if (user is null)
        {
            await _botClient.SendMessage(chatId,
                "❌ Kayıtlı değilsiniz.\n/kayit_alici veya /kayit_satici ile kaydolun.");
            return;
        }

        var role = user.Role == UserRole.Buyer ? "Alıcı" : "Satıcı";
        var brands = user.BrandExpertise.Any()
            ? string.Join(", ", user.BrandExpertise)
            : "Belirtilmemiş";

        var ratingText = user.TotalRatings > 0
            ? $"{user.AverageRating:F1}⭐ ({user.TotalRatings} değerlendirme)"
            : "Henüz değerlendirme yok";

        await _botClient.SendMessage(chatId,
            $"👤 Profiliniz\n" +
            $"{'─',20}\n" +
            $"Kullanıcı: @{user.Username}\n" +
            $"Rol: {role}\n" +
            $"Marka Uzmanlığı: {brands}\n" +
            $"Puan: {ratingText}\n" +
            $"Kayıt: {user.CreatedAt:dd.MM.yyyy}");
    }
}