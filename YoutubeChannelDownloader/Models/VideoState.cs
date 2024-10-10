namespace YoutubeChannelDownloader.Models;

public enum VideoState
{
    /// <summary>
    ///     Видео еще не загружено
    /// </summary>
    NotDownloaded = 0,

    /// <summary>
    ///     Видео успешно загружено
    /// </summary>
    Downloaded = 1,

    /// <summary>
    ///     Ошибка при загрузке
    /// </summary>
    Error = 2,
}
