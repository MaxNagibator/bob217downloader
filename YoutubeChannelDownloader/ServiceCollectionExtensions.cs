using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YoutubeChannelDownloader.Configurations;
using YoutubeChannelDownloader.Services;
using YoutubeExplode;

namespace YoutubeChannelDownloader;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYoutubeChannelDownloader(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DownloadOptions>(configuration.GetSection(nameof(DownloadOptions)))
            .Configure<FFmpegOptions>(configuration.GetSection(nameof(FFmpegOptions)))
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            })
            .AddSingleton<ILoggerFactory>(SerilogFactory.Init)
            .AddSingleton<VideoDownloaderService>()
            .AddSingleton<DirectoryService>()
            .AddSingleton<YoutubeClient>()
            .AddSingleton<DownloadService>()
            .AddSingleton<IYoutubeService, YoutubeService>()
            .AddSingleton<FFmpegConverter>()
            .AddSingleton<FFmpeg>()
            .AddSingleton<HttpClient>()
            .AddSingleton<ChannelService>();

        return services;
    }
}
