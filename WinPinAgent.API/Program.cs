using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using WinPinAgent.API.Bot;
using WinPinAgent.Application.Interfaces;
using WinPinAgent.Application.Services;
using WinPinAgent.Domain.Interfaces;
using WinPinAgent.Infrastructure.Data;
using WinPinAgent.Infrastructure.Repositories;
using WinPinAgent.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Telegram Bot ---
var botToken = builder.Configuration["TelegramBot:Token"]
    ?? throw new InvalidOperationException("Telegram bot token bulunamadý.");
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

// --- Veritabaný ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Repositories ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPartRequestRepository, PartRequestRepository>();
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();

// --- Services ---
builder.Services.AddScoped<IVinParserService, VinParserService>();
builder.Services.AddScoped<IMatchmakingService, MatchmakingService>();
builder.Services.AddScoped<ITelegramNotificationService, TelegramNotificationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

builder.Services.AddHostedService<WinPinAgent.API.BackgroundServices.RequestExpiryService>();
// --- Bot Update Handler ---
builder.Services.AddScoped<BotUpdateHandler>();

builder.Services.AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();

// --- Migration otomatik uygula ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseAuthorization();
app.MapControllers();
app.Run();