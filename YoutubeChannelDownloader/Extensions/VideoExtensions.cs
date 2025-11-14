using System.Text.RegularExpressions;
using YoutubeExplode.Videos;

namespace YoutubeChannelDownloader.Extensions;

public static class VideoExtensions
{
    public static string GetFileName(this IVideo video)
    {
        return video.Title.GetFileName();
    }

    public static string GetFileName(this string video)
    {
        Regex illegalInFileName = new($"[{Regex.Escape(new(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled);
        return illegalInFileName.Replace(video, "_");
    }
}
