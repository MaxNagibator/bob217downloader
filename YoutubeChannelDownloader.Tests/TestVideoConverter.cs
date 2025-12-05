
namespace YoutubeChannelDownloader.Tests;

internal class TestVideoConverter : IVideoConverter
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
