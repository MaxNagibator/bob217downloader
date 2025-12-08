namespace YoutubeChannelDownloader.Services;

public class VideoData
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTimeOffset UploadDate { get; set; }
    public string Thumbnail { get; set; }
}
