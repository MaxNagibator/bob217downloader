using YoutubeChannelDownloader.Models;
using YoutubeChannelDownloader.Services;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeChannelDownloader.Tests;

public class TestYoutubeService(TestYoutubeStorage storage) : IYoutubeService
{
    public ValueTask DownloadAsync(IStreamInfo stream, string path, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async ValueTask DownloadWithProgressAsync(DownloadItemStream downloadStream, CancellationToken token)
    {
        await File.WriteAllTextAsync(downloadStream.FilePath, "huy!", token);
    }

    public async ValueTask DownloadWithProgressAsync(
        IStreamInfo streamInfo,
        string path,
        string streamTitle,
        string videoTitle,
        CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(path, "huy!", cancellationToken);
    }

    public async Task<Channel?> GetChannel(string channelUrl)
    {
        var channel = storage.Channels.FirstOrDefault(x => x.Url == channelUrl);
        if (channel == null)
        {
            throw new Exception("not found");
        }
        return new Channel(new ChannelId("UCUpfL223LhRJuiVJe-uP6hg"), channel.Name, null);
    }

    public async ValueTask<StreamManifest> GetStreamManifestAsync(string url)
    {
        var streams = new List<IStreamInfo>
            {
              new VideoOnlyStreamInfo("x",new Container(), new FileSize(10),new Bitrate(10),".mp4", new VideoQuality(10,10), new Resolution(10,10)),
              new AudioOnlyStreamInfo("x",new Container(), new FileSize(10),new Bitrate(10),".mp4", null, null),
            };
        return new StreamManifest(streams);
    }

    public async IAsyncEnumerable<PlaylistVideo> GetUploadsAsync(string channelUrl)
    {
        if (channelUrl == "UCUpfL223LhRJuiVJe-uP6hg")
        {
            yield return new PlaylistVideo(new VideoId("1"), "варим кашу", null, null, null);
            yield return new PlaylistVideo(new VideoId("2"), "сидим пердим", null, null, null);
        }
    }

    public async ValueTask<Video> GetVideoAsync(string url)
    {
        var video = storage.Videos.FirstOrDefault(x => x.Url == url);
        if (video == null)
        {
            throw new Exception("not found");
        }

        return new Video(
            new VideoId("1"),
            video.Name,
            //video.Channel.Id, todo create channelId
            new Author("UCUpfL223LhRJuiVJe-uP6hg", "channelTwoVideos титле"),
            new DateTime(2025, 12, 01),
            video.Description,
           new TimeSpan(0, 1, 0),
            null,
           [".net one love"],
            // Engagement statistics may be hidden
            new Engagement(
                217,
                1,
                1
            )
        );
    }
}
