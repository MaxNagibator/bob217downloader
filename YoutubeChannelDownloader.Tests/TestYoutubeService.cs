using YoutubeChannelDownloader.Models;
using YoutubeChannelDownloader.Services;
using YoutubeChannelDownloader.Tests.Helpers;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeChannelDownloader.Tests;

public class TestYoutubeService(TestYoutubeStorage storage) : IYoutubeService
{
    public List<string> DownloadedFiles { get; } = [];
    public int DownloadCallCount { get; private set; }

    public ValueTask DownloadAsync(IStreamInfo stream, string path, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        DownloadCallCount++;
        DownloadedFiles.Add(path);

        progress?.Report(0.5);
        progress?.Report(1.0);

        return ValueTask.CompletedTask;
    }

    public async ValueTask DownloadWithProgressAsync(DownloadItemStream downloadStream, CancellationToken token)
    {
        DownloadCallCount++;
        DownloadedFiles.Add(downloadStream.FilePath);
        var directory = Path.GetDirectoryName(downloadStream.FilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(downloadStream.FilePath, "h*y!", token);
    }

    public async ValueTask DownloadWithProgressAsync(
        IStreamInfo streamInfo,
        string path,
        string streamTitle,
        string videoTitle,
        CancellationToken cancellationToken)
    {
        DownloadCallCount++;
        DownloadedFiles.Add(path);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, "h*y!", cancellationToken);
    }

    public Task<Channel?> GetChannel(string channelUrl)
    {
        var channel = storage.Channels.FirstOrDefault(x => x.Url == channelUrl);
        Channel? result = null;

        if (channel != null)
        {
            result = new(channel.Id, channel.Name, []);
        }

        return Task.FromResult(result);
    }

    // TODO: Подвязать манифесты к тестовым видео
    public ValueTask<StreamManifest> GetStreamManifestAsync(string url)
    {
        var streams = new List<IStreamInfo>
        {
            new VideoOnlyStreamInfo(url, new("mp4"), new(10_000_000), new(1000),
                "video/mp4", new(1080, 30), new(1920, 1080)),
            new AudioOnlyStreamInfo(url, new("mp4"), new(1_000_000), new(128),
                "audio/mp4", new Language("en", "English"), null),
        };

        return ValueTask.FromResult(new StreamManifest(streams));
    }

    public async IAsyncEnumerable<PlaylistVideo> GetUploadsAsync(string channelUrl)
    {
        var channel = storage.Channels.FirstOrDefault(x => x.Url == channelUrl || x.Id == channelUrl);
        if (channel == null)
        {
            yield break;
        }

        var videos = storage.Videos.Where(x => x.Channel == channel);
        foreach (var video in videos)
        {
            var author = new Author(channel.Id, channel.Name);
            var thumbnail = new Thumbnail($"https://img.youtube.com/vi/{video.Id}/hqdefault.jpg", new(480, 360));
            yield return new(default,
                video.Id,
                video.Name,
                author,
                video.Duration,
                [thumbnail]);
        }
    }

    public ValueTask<Video> GetVideoAsync(string url)
    {
        var video = storage.Videos.FirstOrDefault(x => x.Url == url || x.Id == url);
        if (video == null)
        {
            throw new TestsException($"Video not found: {url}");
        }

        var channel = video.Channel;

        return ValueTask.FromResult(new Video(new(video.Id),
            video.Name,
            new(new(channel.Id), channel.Name),
            video.UploadDate,
            video.Description,
            video.Duration,
            [new($"https://img.youtube.com/vi/{video.Id}/hqdefault.jpg", new(480, 360))],
            video.Keywords,
            new(video.ViewCount, video.LikeCount, video.DislikeCount)));
    }

    public bool WasFileDownloaded(string path)
    {
        return DownloadedFiles.Contains(path);
    }
}
