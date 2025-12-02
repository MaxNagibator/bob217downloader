using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YoutubeChannelDownloader.Models;
using YoutubeChannelDownloader.Services;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeChannelDownloader.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Assert.Equal(10, 10);
    }

    [Fact]
    public async void TestFail2222()
    {
        var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["DownloadOptions:MaxDownloadsPerRun"] = "1",
            ["DownloadOptions:VideoFolderPath"] = "E:\\bobgroup\\projects\\youtubeDownloader\\tests\\downloads",
        })
        .Build();

        var services = ServiceConfigurator.GetServices(configuration, services =>
        {
            services.AddSingleton<IYoutubeService, TestYoutubeService>();
        });

        var serviceProvider = services.BuildServiceProvider();
        var channelService = serviceProvider.GetRequiredService<ChannelService>();
        await channelService.DownloadVideosAsync("channelTwoVideos");
    }
}

public class TestYoutubeService : IYoutubeService
{
    public ValueTask DownloadAsync(IStreamInfo stream, string path, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask DownloadWithProgressAsync(DownloadItemStream downloadStream, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ValueTask DownloadWithProgressAsync(
        IStreamInfo streamInfo,
        string path,
        string streamTitle,
        string videoTitle,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Channel?> GetChannel(string channelUrl)
    {
        if(channelUrl == "channelTwoVideos")
        {
            return new Channel(new ChannelId("channelTwoVideos"), "Канал с двумя видосами", null);
        }
        throw new NotImplementedException();
    }

    public ValueTask<StreamManifest> GetStreamManifestAsync(string url)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<PlaylistVideo> GetUploadsAsync(string channelUrl)
    {
        if (channelUrl == "channelTwoVideos")
        {
            yield return new PlaylistVideo(new VideoId("1"), "варим кашу", null, null, null);
            yield return new PlaylistVideo(new VideoId("2"), "сидим пердим", null, null, null);
        }
    }

    public ValueTask<Video> GetVideoAsync(string url)
    {
        throw new NotImplementedException();
    }
}
