using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YoutubeChannelDownloader.Services;

namespace YoutubeChannelDownloader.Tests;

public class TestYoutubeServiceTests : BaseTests
{
    [Test]
    public async Task ВозвращаетКаналИзХранилищаПоUrl()
    {
        const string ValidChannelId = "UC1234567890123456789012";

        var channel = _client.WithChannel()
            .SetId(ValidChannelId)
            .SetName("это мой канал. он мне принадлежит")
            .SetUrl("https://www.youtube.com/@my_channel");

        _client.Save();

        var service = new TestYoutubeService(_client.Storage);

        var result = await service.GetChannel("https://www.youtube.com/@my_channel");

        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id.Value, Is.EqualTo(channel.Id));
            Assert.That(result.Title, Is.EqualTo(channel.Name));
        }
    }

    [Test]
    public async Task ВозвращаетВидеоКаналаИзХранилища()
    {
        var channel = _client.WithChannel()
            .SetName("МаксимДваЯйца")
            .SetUrl("https://www.youtube.com/@cooking");

        var topVideo1 = channel.WithVideo().SetName("Рецепт хрючева из красного булдака");
        var topVideo2 = channel.WithVideo().SetName("Пробую царских наполеон");
        _client.Save();

        var service = new TestYoutubeService(_client.Storage);

        var videoTitles = new List<string>();
        await foreach (var video in service.GetUploadsAsync(channel.Url))
        {
            videoTitles.Add(video.Title);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(videoTitles, Has.Count.EqualTo(2));
            Assert.That(videoTitles, Does.Contain(topVideo1.Name));
            Assert.That(videoTitles, Does.Contain(topVideo2.Name));
        }
    }

    [Test]
    public async Task ВозвращаетМетаданныеВидеоИзХранилища()
    {
        // TODO: Подумать над генератором валидных id
        const string ValidVideoId = "CustomVid01";

        var channel = _client.WithChannel();
        var video = channel.WithVideo()
            .SetId(ValidVideoId)
            .SetName("Неправильно рассчитал скорость")
            .SetDescription("полика жалко(")
            .SetDuration(TimeSpan.FromMinutes(45))
            .SetViewCount(500000)
            .SetLikeCount(25000)
            .SetKeywords("tutorial", "real-live");

        _client.Save();

        var service = new TestYoutubeService(_client.Storage);

        var result = await service.GetVideoAsync(video.Url);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id.Value, Is.EqualTo(video.Id));
            Assert.That(result.Title, Is.EqualTo(video.Name));
            Assert.That(result.Description, Is.EqualTo(video.Description));
            Assert.That(result.Duration, Is.EqualTo(video.Duration));
            Assert.That(result.Engagement.ViewCount, Is.EqualTo(video.ViewCount));
            Assert.That(result.Engagement.LikeCount, Is.EqualTo(video.LikeCount));
            Assert.That(result.Keywords, Does.Contain(video.Keywords[0]));
            Assert.That(result.Keywords, Does.Contain(video.Keywords[^1]));
        }
    }

    [Test]
    public async Task ОтслеживаетСкачивания()
    {
        var service = new TestYoutubeService(_client.Storage);
        var filePath = Path.Combine(_tempPath, "test.mp4");

        var manifest = await service.GetStreamManifestAsync("any_url");
        await service.DownloadAsync(manifest.Streams[0], filePath, null, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(service.DownloadCallCount, Is.EqualTo(1));
            Assert.That(service.WasFileDownloaded(filePath), Is.True);
        }
    }
}
