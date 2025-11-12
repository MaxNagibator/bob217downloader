namespace YoutubeChannelDownloader.Configurations;

public class DownloadOptions
{
    private string? _tempFolderPath;
    private string? _videoFolderPath;

    public required string VideoFolderPath { get; init; }

    public string FullVideoFolderPath => _videoFolderPath ??= IsRelativePath
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, VideoFolderPath)
        : VideoFolderPath;

    public string TempFolderPath => _tempFolderPath ??= IsRelativePath
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, VideoFolderPath, ".temp")
        : Path.Combine(VideoFolderPath, ".temp");

    public required bool IsRelativePath { get; init; }

    // TODO: Dry Run при 0
    /// <summary>
    /// Максимальное количество видео, которое можно скачать за один запуск.
    /// </summary>
    /// <remarks>
    /// 0 или отрицательное значение означает отсутствие ограничения.
    /// </remarks>
    public int MaxDownloadsPerRun { get; init; } = 0;

    /// <summary>
    /// Добавлять только новые видео, которые опубликованы после последнего из data.json.
    /// </summary>
    /// <remarks>
    /// Если false, то будет выполнена полная перезапись data.json.
    /// </remarks>
    public bool AddOnlyNew { get; set; } = true;
}
