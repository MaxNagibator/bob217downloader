namespace YoutubeChannelDownloader.Tests.Helpers;

public class TestYoutubeDataClient
{
    public readonly TestYoutubeStorage Storage = new();

    private List<TestObject>? _testObjects = [];

    public void AddObject(TestObject testObject)
    {
        _testObjects?.Add(testObject);
    }

    public TestChannel WithChannel()
    {
        var obj = new TestChannel();
        obj.Attach(this);
        return obj;
    }

    public TestYoutubeDataClient Save()
    {
        if (_testObjects != null)
        {
            foreach (var testObject in _testObjects)
            {
                testObject.SaveObject();
            }
        }

        return this;
    }

    public TestYoutubeDataClient Clear()
    {
        _testObjects = [];

        return this;
    }
}
