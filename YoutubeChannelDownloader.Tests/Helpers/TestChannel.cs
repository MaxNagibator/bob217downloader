namespace YoutubeChannelDownloader.Tests.Helpers;

public class TestChannel : TestObject
{
    public TestChannel()
    {
        Url = "http://bla";
        Name = "Лучик света";
        Title = "Дрисня";
    }

    public string Url { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }

    public override void LocalSave()
    {
        Environment.Storage.Channels.Add(this);
    }

    public TestVideo WithVideo()
    {
        var obj = new TestVideo(this);
        obj.Attach(Environment);
        return obj;
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
}
