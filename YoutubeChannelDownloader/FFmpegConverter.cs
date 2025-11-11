using CliWrap.Builders;

namespace YoutubeChannelDownloader;

public class FFmpegConverter(FFmpeg ffmpeg)
{
    public ValueTask ProcessAsync(
        string filePath,
        IEnumerable<string> streamPaths,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentsBuilder arguments = new();

        foreach (var path in streamPaths)
        {
            arguments.Add("-i").Add(path);
        }

        arguments.Add("-c")
            .Add("copy")
            .Add(filePath);

        arguments.Add("-loglevel")
            .Add("info")
            .Add("-stats");

        arguments.Add("-hide_banner")
            .Add("-threads")
            .Add(Environment.ProcessorCount)
            .Add("-nostdin")
            .Add("-y");

        return ffmpeg.ExecuteAsync(arguments.Build(), progress, cancellationToken);
    }
}
