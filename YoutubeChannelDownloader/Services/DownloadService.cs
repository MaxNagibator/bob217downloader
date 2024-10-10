using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using YoutubeChannelDownloader.Extensions;
using YoutubeChannelDownloader.Models;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeChannelDownloader.Services;

public class DownloadService(
    YoutubeService youtubeService,
    FFmpegConverter converter,
    ILogger<DownloadService> logger)
{
    private readonly List<DownloadItem> _items = [];

    public bool IsNeedDownloadAny => _items.Any(item => item.IsNeedDownloadAnyStream);

    public Result<DownloadItem> FindItem(string id)
    {
        DownloadItem? item = _items.FirstOrDefault(downloadItem => downloadItem.Id == id);
        return item ?? Result.Failure<DownloadItem>($"DownloadItem c id {id} не найден");
    }

    public async Task<(DownloadItem item, DownloadItemStream stream)> DownloadVideo(string url, string path)
    {
        DownloadItem item = await AddToQueueAsync(url, path);
        DownloadItemStream? stream = SetStreamToDownload(item.Id, item.Streams.First().Id).Value;
        await DownloadFromQueue();
        return (item, stream);
    }

    public async Task<DownloadItem> AddToQueueAsync(string url, string path)
    {
        logger.LogDebug("Попытка добавить в очередь: {Url}", url);

        DownloadItem? downloadItem = _items.FirstOrDefault(downloadItem => downloadItem.Url == url);

        if (downloadItem is not null)
        {
            logger.LogDebug("Уже существует в очереди {Id}: {Url}", downloadItem.Id, url);
            return downloadItem;
        }

        Video video = await youtubeService.GetVideoAsync(url);

        StreamManifest streamManifest = await youtubeService.GetStreamManifestAsync(url);

        IAudioStreamInfo highestAudioStream = (IAudioStreamInfo)streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        IVideoStreamInfo highestVideoStream = (IVideoStreamInfo)streamManifest.GetVideoOnlyStreams().GetWithHighestBitrate();

        DownloadItemStream stream = DownloadItemStream.Create(0,
            Path.Combine(path, ".temp"),
            path,
            video,
            highestAudioStream,
            highestVideoStream);

        DownloadItem item = DownloadItem.Create(url, [stream], video).GetValueOrDefault();

        _items.Add(item);
        logger.LogDebug("Добавлено в очередь {Id}: {Url}", item.Id, url);
        return item;
    }

    public Result<DownloadItemStream> SetStreamToDownload(string downloadId, int streamId)
    {
        logger.LogDebug("Попытка установить поток для скачивания: {Id} {StreamId}", downloadId, streamId);

        return FindItem(downloadId)
            .TapError(s => logger.LogError("Не удалось найти элемент загрузки: {Id}, Ошибка: {Error}", downloadId, s))
            .Bind(item => item.GetStream(streamId))
            .TapError(s => logger.LogError("Не удалось получить поток: {StreamId} для элемента: {Id}, Ошибка: {Error}", streamId, downloadId, s))
            .Tap(stream => stream.State = DownloadItemState.Wait)
            .Tap(() => logger.LogDebug("Успешно установлен поток для скачивания: {Id} {StreamId}", downloadId, streamId));
    }

    public async Task DownloadFromQueue()
    {
        DownloadItem? downloadItem = _items.FirstOrDefault(x => x.Streams.Any(itemSteam => itemSteam.State == DownloadItemState.Wait));
        DownloadItemStream? downloadStream = downloadItem?.GetWaitStreams().FirstOrDefault();

        if (downloadStream == null || downloadItem == null)
        {
            logger.LogWarning("Очередь скачивания пустая");
            return;
        }

        downloadStream.State = DownloadItemState.InProcess;
        logger.LogDebug("Попытка скачать из очереди: {Id} {StreamId}", downloadItem.Id, downloadStream.Id);

        CancellationTokenSource cancellationTokenSource = new();

        try
        {
            await DownloadCombinedStream(downloadStream, downloadItem, cancellationTokenSource.Token);
            downloadStream.State = DownloadItemState.Ready;
            logger.LogDebug("Успешно скачан из очереди: {Id} {StreamId}", downloadItem.Id, downloadStream.Id);
        }
        catch (Exception exception)
        {
            await cancellationTokenSource.CancelAsync();

            downloadStream.State = DownloadItemState.Error;
            logger.LogError(exception, "Не полупилось скачать из очереди: {Id} {StreamId}", downloadItem.Id, downloadStream.Id);
        }
    }

    private async Task DownloadCombinedStream(DownloadItemStream downloadStream, DownloadItem downloadItem, CancellationToken cancellationToken = default)
    {
        string audioPath = downloadStream.TempPath.AddSuffixToFileName("audio");
        string videoPath = downloadStream.TempPath.AddSuffixToFileName("video");

        if (downloadStream.AudioStreamInfo == null || downloadStream.VideoStreamInfo == null)
        {
            logger.LogError("Не удалось объединить ({MethodName}). Нет видео или аудио", nameof(DownloadCombinedStream));
            return;
        }

        ValueTask audioTask = youtubeService.DownloadWithProgressAsync(downloadStream.AudioStreamInfo,
            audioPath,
            downloadStream.Title,
            downloadItem.Video.Title,
            cancellationToken);

        ValueTask videoTask = youtubeService.DownloadWithProgressAsync(downloadStream.VideoStreamInfo,
            videoPath,
            downloadStream.Title,
            downloadItem.Video.Title,
            cancellationToken);

        await Task.WhenAll(audioTask.AsTask(), videoTask.AsTask());

        logger.LogDebug("Попытка объединить видео и аудио: {Id} {StreamId}", downloadItem.Id, downloadStream.Id);

        double oldPercent = -1;

        Progress<double> progress = new(percent =>
        {
            if (percent - oldPercent < 0.1)
            {
                return;
            }

            logger.LogDebug("Объединение: {Percent:P2}", percent);
            oldPercent = percent;
        });

        await converter.ProcessAsync(downloadStream.FilePath, [audioPath, videoPath], progress, cancellationToken);

        logger.LogDebug("Успешно объединено видео и аудио: {Id} {StreamId}", downloadItem.Id, downloadStream.Id);
    }
}
