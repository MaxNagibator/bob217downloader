namespace YoutubeChannelDownloader.Tests.Helpers;

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
