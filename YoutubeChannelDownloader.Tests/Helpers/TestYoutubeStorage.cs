namespace YoutubeChannelDownloader.Tests.Helpers;

/// <summary>
/// Имитация хранилища (типо таблички БД).
/// </summary>
public class TestYoutubeStorage
{
    public List<TestChannel> Channels { get; set; } = [];
    public List<TestVideo> Videos { get; set; } = [];
}
