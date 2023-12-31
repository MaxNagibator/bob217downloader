﻿using System.IO;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Logic
{
    public class YoutubeDownloader
    {
        public static async Task Download(IStreamInfo stream, string path)
        {
            var youtube = new YoutubeClient();
            await youtube.Videos.Streams.DownloadAsync(stream, path);
        }
    }
}
