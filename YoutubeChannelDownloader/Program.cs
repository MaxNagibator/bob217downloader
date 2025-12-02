using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YoutubeChannelDownloader;
using YoutubeChannelDownloader.Configurations;
using YoutubeChannelDownloader.Services;
using YoutubeExplode;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.Development.json", true, true)
    .Build();

var services = ServiceConfigurator.GetServices(configuration);
var serviceProvider = services.BuildServiceProvider();
var service = serviceProvider.GetRequiredService<ChannelService>();

var channelUrl = configuration["Channel:Url"];

if (string.IsNullOrWhiteSpace(channelUrl))
{
    throw new InvalidOperationException("URL канала не настроен. Пожалуйста, укажите 'Channel:Url' в appsettings.");
}

await service.DownloadVideosAsync(channelUrl);

Log.CloseAndFlush();

public class ServiceConfigurator
{
    public static IServiceCollection GetServices(IConfiguration configuration, Action<IServiceCollection>? configureServices = null)
    {
        var serviceProvider = new ServiceCollection()
        .Configure<DownloadOptions>(configuration.GetSection(nameof(DownloadOptions)))
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

        configureServices?.Invoke(serviceProvider);
        return serviceProvider;
    }
}
