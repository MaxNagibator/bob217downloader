using CSharpFunctionalExtensions;
using YoutubeExplode.Videos;

namespace YoutubeChannelDownloader.Models;

public class DownloadItem
{
    private readonly List<DownloadItemStream> _streams;

    private DownloadItem(string id, string url, IEnumerable<DownloadItemStream> streams, Video video)
    {
        Id = id;
        Url = url;
        _streams = streams.ToList();
        Video = video;
    }

    public string Id { get; }
    public string Url { get; }

    public IEnumerable<DownloadItemStream> Streams => _streams;

    public Video Video { get; }

    public bool IsNeedDownloadAnyStream => Streams.Any(stream => stream.IsNeedDownload);

    public static Result<DownloadItem> Create(string url, IEnumerable<DownloadItemStream> streams, Video video)
    {
        return Result.FailureIf(string.IsNullOrWhiteSpace(url), "URL не может быть null или пустым")
            .Map(streams.ToList)
            .Ensure(list => list.Count != 0, "Список потоков не может быть пустым")
            .Map(list => new DownloadItem(video.Id.Value, url, list, video));
    }

    public Result<DownloadItemStream> GetStream(int id)
    {
        var itemSteam = Streams.FirstOrDefault(downloadItem => downloadItem.Id == id);
        return Result.FailureIf(itemSteam == null, itemSteam, $"DownloadItemStream c id {id} не найден")!;
    }

    public IEnumerable<DownloadItemStream> GetWaitStreams()
    {
        return Streams.Where(stream => stream.State == DownloadItemState.Wait);
    }
}
