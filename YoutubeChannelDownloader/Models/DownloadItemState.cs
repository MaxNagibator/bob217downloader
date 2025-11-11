namespace YoutubeChannelDownloader.Models;

public enum DownloadItemState
{
    /// <summary>
    /// Ожидает дальнейших распоряжений.
    /// </summary>
    Added,

    /// <summary>
    /// Ожидает загрузку.
    /// </summary>
    Wait,

    /// <summary>
    /// В процессе загрузки.
    /// </summary>
    InProcess,

    /// <summary>
    /// Загружен.
    /// </summary>
    Ready,

    /// <summary>
    /// Ошибка.
    /// </summary>
    Error,
}
