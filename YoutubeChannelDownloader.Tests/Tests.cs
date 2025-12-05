using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YoutubeChannelDownloader.Services;
using YoutubeChannelDownloader.Tests.Helpers;

namespace YoutubeChannelDownloader.Tests;

public class TestYoutubeServiceTests
{
    private TestYoutubeDataClient _client = null!;
    private string _baseTempPath = Path.Combine(Path.GetTempPath(), "YoutubeChannelDownloader");
    private string _tempPath = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        try
        {
            if (Directory.Exists(_baseTempPath))
            {
                Directory.Delete(_baseTempPath, true);
            }
        }
        catch (IOException)
        {
        }
    }

    [SetUp]
    public void Setup()
    {
        _client = new();
        //TODO: Шанс коллизии
        _tempPath = Path.Combine(_baseTempPath, "YoutubeTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempPath);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Clear();

        // TODO: Подумать
        try
        {
            if (Directory.Exists(_tempPath))
            {
                // Directory.Delete(_tempPath, true);
            }
        }
        catch (IOException)
        {
        }
    }

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

    /// <summary>
    /// Создаём тестовые данные билдером,
    /// TestYoutubeService возвращает их как будто от YouTube API,
    /// ChannelService работает с этими данными.
    /// isDownload: false - только сканируем канал без скачивания (без FFmpeg)
    /// </summary>
    [Test]
    public async Task КаналаИспользуетТестовыеДанныеДляСканирования()
    {
        const string Video1Id = "TestVideo01";
        const string Video2Id = "TestVideo02";

        var channel = _client.WithChannel()
            .SetName("TestChannel")
            .SetUrl("https://www.youtube.com/@test_channel");

        var video1 = channel.WithVideo()
              .SetId(Video1Id)
              .SetName("Первое видео");

        var video2 = channel.WithVideo()
             .SetId(Video2Id)
             .SetName("Второе видео");

        _client.Save();

        var testYoutubeService = new TestYoutubeService(_client.Storage);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DownloadOptions:MaxDownloadsPerRun"] = "10",
                ["DownloadOptions:VideoFolderPath"] = _tempPath,
            })
            .Build();

        var services = new ServiceCollection()
            .AddYoutubeChannelDownloader(configuration)
            .AddSingleton<IYoutubeService>(testYoutubeService);

        var provider = services.BuildServiceProvider();
        var channelService = provider.GetRequiredService<ChannelService>();

        await channelService.DownloadVideosAsync(channel.Url, false);

        using (Assert.EnterMultipleScope())
        {
            var channelDir = Path.Combine(_tempPath, channel.Name);
            Assert.That(Directory.Exists(channelDir), Is.True);

            var dataFile = Path.Combine(channelDir, "data.json");
            Assert.That(File.Exists(dataFile), Is.True);

            var dataContent = await File.ReadAllTextAsync(dataFile);

            Assert.That(dataContent, Does.Contain(video1.Id));
            Assert.That(dataContent, Does.Contain(video1.Name));

            Assert.That(dataContent, Does.Contain(video2.Id));
            Assert.That(dataContent, Does.Contain(video2.Name));
        }
    }

    [Test]
    public async Task СкачатьВидео()
    {
        var channel = _client.WithChannel()
            .SetName("TestChannel")
            .SetUrl("https://www.youtube.com/@test_channel")
            .WithVideo()
            .SetName("Единственное видео")
            .Channel;
        _client.Save();

        var testYoutubeService = new TestYoutubeService(_client.Storage);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DownloadOptions:MaxDownloadsPerRun"] = "10",
                ["DownloadOptions:VideoFolderPath"] = _tempPath,
            })
            .Build();

        var services = new ServiceCollection()
            .AddYoutubeChannelDownloader(configuration)
            .AddSingleton<IYoutubeService>(testYoutubeService)
            .AddSingleton<IVideoConverter, TestVideoConverter>();

        var provider = services.BuildServiceProvider();
        var channelService = provider.GetRequiredService<ChannelService>();

        await channelService.DownloadVideosAsync(channel.Url, true);

        using (Assert.EnterMultipleScope())
        {
            var channelDir = Path.Combine(_tempPath, channel.Name);
            Assert.That(Directory.Exists(channelDir), Is.True);

            var dataFile = Path.Combine(channelDir, "data.json");
            Assert.That(File.Exists(dataFile), Is.True);

            var videoFile = Path.Combine(channelDir, "videos", $"{channel.Videos[0].Name}.mp4");
            Assert.That(File.Exists(videoFile), Is.True);
        }
    }
}
