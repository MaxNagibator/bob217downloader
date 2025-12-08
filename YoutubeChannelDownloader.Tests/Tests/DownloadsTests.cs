namespace YoutubeChannelDownloader.Tests;

public class DownloadsTests : BaseTests
{

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

        await GetChannelService().DownloadVideosAsync(channel.Url, true);

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

        await GetChannelService().DownloadVideosAsync(channel.Url, true);

        using (Assert.EnterMultipleScope())
        {
            var channelDir = Path.Combine(_tempPath, channel.Name);
            Assert.That(Directory.Exists(channelDir), Is.True);

            var dataFile = Path.Combine(channelDir, "data.json");
            Assert.That(File.Exists(dataFile), Is.True);

            var videoFile = Path.Combine(channelDir, "videos", $"{channel.Videos[0].Id}.mp4");
            Assert.That(File.Exists(videoFile), Is.True);

            var videoMetadataFile = Path.Combine(channelDir, "videos", $"{channel.Videos[0].Id}.json");
            Assert.That(File.Exists(videoMetadataFile), Is.True);

            var thumbnailFile = Path.Combine(channelDir, "videos", $"{channel.Videos[0].Id}_thumbnail.jpg");
            Assert.That(File.Exists(thumbnailFile), Is.True);
        }
    }
}
