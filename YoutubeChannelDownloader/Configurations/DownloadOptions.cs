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
}
