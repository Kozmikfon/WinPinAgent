using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WinPinAgent.Application.Interfaces;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Domain.Interfaces;
using WinPinAgent.Domain.Enums;

using DomainUser = WinPinAgent.Domain.Entities.User;
using TelegramUser = Telegram.Bot.Types.User;

namespace WinPinAgent.API.Bot;

public class BotUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IVinParserService _vinParser;
    private readonly IMatchmakingService _matchmaking;
    private readonly IUserRepository _userRepo;
    private readonly IPartRequestRepository _requestRepo;
    private readonly  IOfferRepository _offerRepo;

    public BotUpdateHandler(
        ITelegramBotClient botClient,
        IVinParserService vinParser,
        IMatchmakingService matchmaking,
        IUserRepository userRepo,
        IPartRequestRepository requestRepo,
        IOfferRepository offerRepo)
    {
        _botClient = botClient;
        _vinParser = vinParser;
        _matchmaking = matchmaking;
        _userRepo = userRepo;
        _requestRepo = requestRepo;
        _offerRepo = offerRepo;
    }

    public async Task HandleAsync(Update update)
    {
        if (update.Message is not { Text: { } text } message) return;

        var chatId = message.Chat.Id;
        var args = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = args[0].ToLower();

        switch (command)
        {
            case "/start":
                await HandleStartAsync(chatId, message.From);
                break;
            case "/kayit_alici":
                await HandleRegisterBuyerAsync(chatId, message.From);
                break;
            case "/kayit_satici":
                await HandleRegisterSellerAsync(chatId, args, message.From);
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

    private async Task HandleStartAsync(long chatId, TelegramUser? from)
    {
        await _botClient.SendMessage(chatId,
            "👋 WinPin'e hoş geldiniz!\n\n" +
            "Alıcıysanız: /kayit_alici\n" +
            "Satıcıysanız: /kayit_satici BMW AUDI");
    }

    private async Task HandleRegisterBuyerAsync(long chatId, TelegramUser? telegramUser)
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
            Username = telegramUser?.Username ?? "bilinmiyor",
            Role = UserRole.Buyer
        };

        await _userRepo.AddAsync(user);
        await _botClient.SendMessage(chatId, "✅ Alıcı olarak kaydoldunuz.");
    }

    private async Task HandleRegisterSellerAsync(long chatId, string[] args, TelegramUser? telegramUser)
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
            Username = telegramUser?.Username ?? "bilinmiyor",
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
        await _botClient.SendMessage(chatId,
            $"✅ Talebiniz oluşturuldu!\n" +
            $"Araç: {brand}\n" +
            $"Parça: {partName}\n" +
            $"Talep No: {request.Id}\n\n" +
            $"İlgili tedarikçiler bilgilendiriliyor...");

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

        if (request.BuyerId != chatId)
        {
            await _botClient.SendMessage(chatId, "❌ Bu talep size ait değil.");
            return;
        }

        if (!request.Offers.Any())
        {
            await _botClient.SendMessage(chatId,
                $"📭 Henüz teklif gelmedi.\n" +
                $"Durum: {request.Status}\n" +
                $"Son geçerlilik: {request.ExpiresAt:dd.MM.yyyy HH:mm}");
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"📋 Teklifler — {request.PartName} ({request.Brand})");
        sb.AppendLine($"Durum: {request.Status}\n");

        int i = 1;
        foreach (var offer in request.Offers)
        {
            sb.AppendLine($"{i}. Teklif");
            sb.AppendLine($"   Fiyat: {offer.Price:C}");
            sb.AppendLine($"   Stok: {offer.StockStatus}");
            sb.AppendLine($"   Kabul için: /kabul {requestId} {offer.Id}\n");
            i++;
        }

        await _botClient.SendMessage(chatId, sb.ToString());
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
    }
}