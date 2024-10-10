using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YoutubeChannelDownloader;
using YoutubeChannelDownloader.Configurations;
using YoutubeChannelDownloader.Services;
using YoutubeExplode;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.Development.json", true, true)
    .Build();

ServiceProvider serviceProvider = new ServiceCollection()
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
    .AddSingleton<YoutubeService>()
    .AddSingleton<FFmpegConverter>()
    .AddSingleton<FFmpeg>()
    .AddSingleton<HttpClient>()
    .AddSingleton<ChannelService>()
    .BuildServiceProvider();

ChannelService service = serviceProvider.GetRequiredService<ChannelService>();

string channelId = "https://www.youtube.com/@bobito217";
// string channelId = "UCOuW8i824NprPKrM4Pq4R0w";// боксёр

await service.DownloadVideosAsync(channelId);

Log.CloseAndFlush();
