namespace YoutubeChannelDownloader.Models;

public class VideoInfo(string id, string title, string fileName, VideoState state, string url, string? thumbnailUrl)
{
    /// <summary>
    /// ID видео на YouTube.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Заголовок видео.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Имя файла, под которым будет сохранено видео.
    /// </summary>
    public string FileName { get; } = fileName;

    /// <summary>
    /// Текущее состояние процесса загрузки видео.
    /// </summary>
    public VideoState State { get; set; } = state;

    /// <summary>
    /// URL видео на YouTube.
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    /// URL миниатюры видео.
    /// </summary>
    public string? ThumbnailUrl { get; } = thumbnailUrl;
}
