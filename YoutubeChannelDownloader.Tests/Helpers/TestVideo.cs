namespace YoutubeChannelDownloader.Tests.Helpers;

public class TestVideo : TestObject
{
    private static int _counter;

    public TestVideo(TestChannel channel)
    {
        Channel = channel;
        var id = ++_counter;
        Id = $"TestVid{id:D4}";
        Name = $"Видео {id}";
        Url = $"https://www.youtube.com/watch?v={Id}";
        Description = "Видео обучающее с котами";
        Duration = TimeSpan.FromMinutes(5);
        UploadDate = new(2025, 1, 1);
        ViewCount = 100;
        LikeCount = 10;
        DislikeCount = 1;
        Keywords = ["test", "video"];
    }

    public string Id { get; set; }
    public TestChannel Channel { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime UploadDate { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long DislikeCount { get; set; }
    public IReadOnlyList<string> Keywords { get; set; }

    public override void LocalSave()
    {
        Environment.Storage.Videos.Add(this);
    }

    public TestVideo SetId(string value)
    {
        Id = value;
        Url = $"https://www.youtube.com/watch?v={Id}";
        return this;
    }

    public TestVideo SetName(string value)
    {
        Name = value;
        return this;
    }

    public TestVideo SetUrl(string value)
    {
        Url = value;
        return this;
    }

    public TestVideo SetDescription(string value)
    {
        Description = value;
        return this;
    }

    public TestVideo SetDuration(TimeSpan value)
    {
        Duration = value;
        return this;
    }

    public TestVideo SetUploadDate(DateTime value)
    {
        UploadDate = value;
        return this;
    }

    public TestVideo SetViewCount(long value)
    {
        ViewCount = value;
        return this;
    }

    public TestVideo SetLikeCount(long value)
    {
        LikeCount = value;
        return this;
    }

    public TestVideo SetDislikeCount(long value)
    {
        DislikeCount = value;
        return this;
    }

    public TestVideo SetKeywords(params string[] value)
    {
        Keywords = value;
        return this;
    }

    public TestVideo WithAnotherVideo()
    {
        return Channel.WithVideo();
    }
}
