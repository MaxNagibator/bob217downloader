
using YoutubeChannelDownloader.Services;

namespace YoutubeChannelDownloader.Tests;

public class TestVideoConverter : IVideoConverter
{
    public ValueTask MergeMediaAsync(
        string filePath,
        IEnumerable<string> streamPaths,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        File.WriteAllText(filePath, "я склеился");
        return ValueTask.CompletedTask;
    }
}

public class TestPictureDownloader : IPictureDownloader
{
    public async Task Download(string url, string path)
    {
        await File.WriteAllTextAsync(path, url);
    }
}
