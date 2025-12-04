namespace YoutubeChannelDownloader.Tests.Helpers;

public class TestVideo : TestObject
{
    public TestVideo(TestChannel channel)
    {
        Channel = channel;
        Name = "Видео 1";
        Url = "http://bla/1";
        Description = "Видео обучающее с котами";
    }

    public TestChannel Channel { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }

    public override void LocalSave()
    {
        Environment.Storage.Videos.Add(this);
    }
}
