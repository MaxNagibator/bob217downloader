using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YoutubeChannelDownloader.Services;

namespace YoutubeChannelDownloader.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public async Task Test1()
    {
        var client = new TestYoutubeDataClient();
        var channel = client.WithChannel()
            .SetName("Канал " + DateTime.Now.ToString("yyyyMMddHHmmss"))
            .SetUrl("https://www.youtube.com/@bobito217");
        var video = channel.WithVideo();
        client.Save();

        var testYoutubeClient = new TestYoutubeService(client.Storage);

        var path = "E:\\bobgroup\\projects\\youtubeDownloader\\tests\\downloads";
        var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["DownloadOptions:MaxDownloadsPerRun"] = "1",
            ["DownloadOptions:VideoFolderPath"] = path,
        })
        .Build();

        var services = ServiceConfigurator.GetServices(configuration, services =>
        {
            services.AddSingleton<IYoutubeService>(testYoutubeClient);
        });

        var serviceProvider = services.BuildServiceProvider();
        var channelService = serviceProvider.GetRequiredService<ChannelService>();
        await channelService.DownloadVideosAsync(channel.Url);

        var expectedDataPath = Path.Combine(path, channel.Name, "data.json");
        var expectedVideoPath = Path.Combine(path, channel.Name, "videos", video.Name + "_title.txt.");
        var dataContent = File.ReadAllText(expectedDataPath);
        var videoTitleContent = File.ReadAllText(expectedVideoPath);
    }
}
