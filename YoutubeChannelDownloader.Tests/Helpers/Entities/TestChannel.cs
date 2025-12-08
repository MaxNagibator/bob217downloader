namespace YoutubeChannelDownloader.Tests.Helpers;

public class TestChannel : TestObject
{
    private static int _counter;
    private readonly List<TestVideo> _videos = [];

    public TestChannel()
    {
        var id = ++_counter;
        Id = $"UCTestChannel{id:D11}";
        Url = $"https://www.youtube.com/@channel{id}";
        Name = $"Канал {id}";
        Title = $"Заголовок канала {id}";
    }

    public string Id { get; set; }
    public string Url { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public IReadOnlyList<TestVideo> Videos => _videos;

    public override void LocalSave()
    {
        Environment.Storage.Channels.Add(this);
    }

    public TestVideo WithVideo()
    {
        var obj = new TestVideo(this);
        obj.Attach(Environment);
        _videos.Add(obj);
        return obj;
    }

    public TestChannel SetId(string value)
    {
        Id = value;
        return this;
    }

    public TestChannel SetName(string value)
    {
        Name = value;
        return this;
    }

    public TestChannel SetUrl(string value)
    {
        Url = value;
        return this;
    }

    public TestChannel SetTitle(string value)
    {
        Title = value;
        return this;
    }
}
