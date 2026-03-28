using DevBites.Services;
using DevBites.ViewModels;
using DevBites.Views;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Microsoft.Extensions.Logging;

namespace DevBites
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            DotEnvLoader.Load();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // AI – switch provider via AI_PROVIDER in .env
            IChatClient chatClient = Constants.AiProvider.ToLowerInvariant() switch
            {
                "ollama" => new OllamaApiClient(Constants.OllamaEndpoint, Constants.OllamaModel),
                _ => new GeminiChatClient(new GeminiClientOptions
                {
                    ApiKey = Constants.GeminiApiKey,
                    ModelId = Constants.GeminiModel
                })
            };
            builder.Services.AddSingleton(chatClient);

            // Services
            builder.Services.AddSingleton<IAIContentService, AIContentService>();
            builder.Services.AddSingleton<IFavoritesService, FavoritesService>();

            // ViewModels
            builder.Services.AddTransient<FeedViewModel>();

            // Pages
            builder.Services.AddTransient<FeedPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
