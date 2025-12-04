using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using YoutubeChannelDownloader;
using YoutubeChannelDownloader.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.Development.json", true, true)
    .Build();

var services = new ServiceCollection()
    .AddYoutubeChannelDownloader(configuration);

var serviceProvider = services.BuildServiceProvider();
var service = serviceProvider.GetRequiredService<ChannelService>();

var channelUrl = configuration["Channel:Url"];

if (string.IsNullOrWhiteSpace(channelUrl))
{
    throw new InvalidOperationException("URL канала не настроен. Пожалуйста, укажите 'Channel:Url' в appsettings.");
}

await service.DownloadVideosAsync(channelUrl);

Log.CloseAndFlush();
