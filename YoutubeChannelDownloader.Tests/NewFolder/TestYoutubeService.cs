
namespace YoutubeChannelDownloader.Tests;

public class TestYoutubeDataClient
{
    public TestYoutubeStorage Storage = new TestYoutubeStorage();

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

/// <summary>
/// Имитация хранилища (типо таблички БД).
/// </summary>
public class TestYoutubeStorage
{
    public List<TestChannel> Channels { get; set; } = new List<TestChannel>();
    public List<TestVideo> Videos { get; set; } = new List<TestVideo>();
}

public class TestChannel : TestObject
{
    public string Url { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }

    public TestChannel()
    {
        Url = "http://bla";
        Name = "Лучик света";
        Title = "Дрисня";
    }

    public TestVideo WithVideo()
    {
        var obj = new TestVideo(this);
        obj.Attach(this.Environment);
        return obj;
    }

    public override void LocalSave()
    {
        Environment.Storage.Channels.Add(this);
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

public class TestVideo : TestObject
{
    public TestChannel Channel { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }

    public TestVideo(TestChannel channel)
    {
        Channel = channel;
        Name = "Видео 1";
        Url = "http://bla/1";
        Description = "Видео обучающее с котами";
    }
    public override void LocalSave()
    {
        Environment.Storage.Videos.Add(this);
    }
}

public abstract class TestObject
{
    private readonly List<TestObject> _objects = [];

    public TestYoutubeDataClient Environment { get; private set; } = null!;
    protected bool IsNew { get; set; } = true;

    public virtual void Attach(TestYoutubeDataClient env)
    {
        Environment = env;
        AfterAttach();
        env.AddObject(this);
    }

    public virtual void AfterAttach()
    {
    }

    public abstract void LocalSave();

    public TestObject SaveObject()
    {
        LocalSave();

        foreach (var testObject in _objects)
        {
            testObject.SaveObject();
        }

        return this;
    }
}
