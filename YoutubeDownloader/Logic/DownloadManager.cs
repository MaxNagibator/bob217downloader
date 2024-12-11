using System.Diagnostics;
using System.Text;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Logic
{
    public class DownloadManager
    {
        public List<DownloadItem> Items { get; set; }
        private ILogger<DownloadManager> _logger;

        public DownloadManager(ILogger<DownloadManager> logger)
        {
            Items = new List<DownloadItem>();
            _logger = logger;
        }

        public async Task<DownloadItem> AddToQueueAsync(string url)
        {
            _logger.LogTrace("Try add to queue: " + url);
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(url);

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);

            var id = Guid.NewGuid();

            var streams = streamManifest.Streams.Select((x, i) =>
                    new DownloadItemSteam
                    {
                        Id = i,
                        Name = id.ToString() + "_" + i + ".mp4",
                        FullPath = Path.Combine(Globals.Settings.VideoFolderPath, id.ToString() + "_" + i + ".mp4"),
                        Stream = streamManifest.Streams[i],
                        State = DownloadItemState.Base
                    }).ToList();
            var streamId = streams.Count();

            var types = new List<string> { "webm", "mp4" };
            foreach (var type in types)
            {
                var bestMuxedStream = streams.Where(x => x.Stream != null && x.Stream.Container.Name == type && x.Stream is MuxedStreamInfo)
                    .OrderByDescending(x => ((MuxedStreamInfo)x.Stream).VideoQuality.MaxHeight).FirstOrDefault();
                var bestVideoStream = streams.Where(x => x.Stream != null && x.Stream.Container.Name == type && x.Stream is VideoOnlyStreamInfo)
                    .OrderByDescending(x => ((VideoOnlyStreamInfo)x.Stream).VideoQuality.MaxHeight).FirstOrDefault();
                var bestAudioStream = streams.Where(x => x.Stream != null && x.Stream.Container.Name == type && x.Stream is AudioOnlyStreamInfo)
                    .OrderByDescending(x => ((AudioOnlyStreamInfo)x.Stream).Size).FirstOrDefault();
                int muxedMaxHeight = 0;
                if (bestMuxedStream != null)
                {
                    muxedMaxHeight = ((MuxedStreamInfo)bestMuxedStream.Stream).VideoQuality.MaxHeight;
                }

                if (bestVideoStream != null && bestAudioStream != null)
                {
                    if (muxedMaxHeight < ((VideoOnlyStreamInfo)bestVideoStream.Stream).VideoQuality.MaxHeight)
                    {
                        streams.Insert(0, new DownloadItemSteam
                        {
                            Id = streamId,
                            Name = id.ToString() + "_" + streamId + "." + type,
                            FullPath = Path.Combine(Globals.Settings.VideoFolderPath, id.ToString() + "_" + streamId + "." + type),
                            State = DownloadItemState.Base,
                            IsCombineAfterDownload = true,
                            CombineAfterDownloadStreamAudio = bestAudioStream.Stream,
                            CombineAfterDownloadStreamVideo = bestVideoStream.Stream,
                        });
                        streamId++;
                    }
                }
            }


            var item = new DownloadItem
            {
                Id = id,
                Url = url,
                Video = video,
                Streams = streams,
            };
            Items.Add(item);
            _logger.LogTrace("Add to queue: " + url + " " + id);
            return item;
        }

        public void SetStreamToDownload(Guid downloadId, int streamId, Action afterDownloadAction = null)
        {
            _logger.LogTrace("Try set stream to download: " + downloadId + " " + streamId);
            var downloadItem = Items.First(x => x.Id == downloadId);
            var stream = downloadItem.Streams.First(x => x.Id == streamId);
            stream.State = DownloadItemState.Wait;
            stream.AfterDownloadAction = afterDownloadAction;
            _logger.LogTrace("Set stream to download: " + downloadId + " " + streamId);
        }

        public async Task DownloadFromQueue()
        {
            var downloadItem = Items.FirstOrDefault(x => x.Streams.Any(x => x.State == DownloadItemState.Wait));
            if (downloadItem != null)
            {
                var downloadStream = downloadItem.Streams.First(x => x.State == DownloadItemState.Wait);
                downloadStream.State = DownloadItemState.InProcess;
                _logger.LogTrace("Try download from queue: " + downloadItem.Id + " " + downloadStream.Id);

                try
                {
                    if (downloadStream.IsCombineAfterDownload)
                    {
                        var audioPath = downloadStream.FullPath + "_audio." + downloadStream.CombineAfterDownloadStreamVideo.Container.Name;
                        var videoPath = downloadStream.FullPath + "_video." + downloadStream.CombineAfterDownloadStreamVideo.Container.Name;
                        var task = YoutubeDownloader.Download(downloadStream.CombineAfterDownloadStreamAudio, audioPath);
                        var task2 = YoutubeDownloader.Download(downloadStream.CombineAfterDownloadStreamVideo, videoPath);
                        Task.WaitAll(task, task2);

                        _logger.LogTrace("Try merge video and audio: " + downloadItem.Id + " " + downloadStream.Id);
                        var args = "-i \"" + videoPath + "\" -i \"" + audioPath + "\" -c copy \"" + downloadStream.FullPath + "\"";
                        await RunAsync(args);
                    }
                    else
                    {
                        await YoutubeDownloader.Download(downloadStream.Stream, downloadStream.FullPath);
                    }
                    downloadStream.State = DownloadItemState.Ready;
                    if (downloadStream.AfterDownloadAction != null)
                    {
                        try
                        {
                            // todo пока отправляем видева пользователю, следующий видос не качается
                            downloadStream.AfterDownloadAction();
                        }
                        catch
                        {
                            // todo обработку ошибок доверим автору колбэка, а тут как бы на всякий влепим
                        }
                    }
                    _logger.LogTrace("Download from queue: " + downloadItem.Id + " " + downloadStream.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Download from queue: " + downloadItem.Id + " " + downloadStream.Id + " " + ex.Message);
                    downloadStream.State = DownloadItemState.Error;
                }
            }
        }

        public async Task RunAsync(string ffmpegCommand)
        {
            using Process process = new Process();
            ProcessStartInfo processStartInfo2 = (process.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "C:\\Services\\utils\\ffmpeg\\ffmpeg.exe",
                RedirectStandardError = true,
                Arguments = ffmpegCommand
            });
            process.Start();


            string lastLine = null;
            StringBuilder runMessage = new StringBuilder();
            while (!process.StandardError.EndOfStream)
            {
                string text = await process.StandardError.ReadLineAsync().ConfigureAwait(continueOnCapturedContext: false);
                runMessage.AppendLine(text);
                lastLine = text;
            }

            await process.WaitForExitAsync().ConfigureAwait(continueOnCapturedContext: false);
            if (process.ExitCode != 0)
            {
            }
        }

        public class DownloadItem
        {
            public Guid Id { get; set; }
            public string Url { get; set; }
            public List<DownloadItemSteam> Streams { get; set; }
            public Video Video { get; internal set; }
        }

        public class DownloadItemSteam
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string FullPath { get; set; }
            public DownloadItemState State { get; set; }
            public IStreamInfo Stream { get; set; }

            public bool IsCombineAfterDownload { get; set; }
            public IStreamInfo CombineAfterDownloadStreamAudio { get; set; }
            public IStreamInfo CombineAfterDownloadStreamVideo { get; set; }


            public string Title
            {
                get
                {
                    if (IsCombineAfterDownload)
                    {
                        var video = (VideoOnlyStreamInfo)CombineAfterDownloadStreamVideo;
                        return "Muxed (" + video.VideoQuality.MaxHeight + " | " + video.Container.Name + ") ~" + SizeMB + "МБ";
                    }
                    else
                    {
                        return Stream.ToString() + " " + SizeMB + "МБ";
                    }
                }
            }

            public double SizeMB
            {
                get
                {
                    if (IsCombineAfterDownload)
                    {
                        var size = CombineAfterDownloadStreamAudio.Size.MegaBytes + CombineAfterDownloadStreamVideo.Size.MegaBytes;
                        return Math.Round(size, 2);
                    }
                    else
                    {
                        return Math.Round(Stream.Size.MegaBytes, 2);
                    }
                }
            }

            public string VideoType
            {
                get
                {
                    if (IsCombineAfterDownload)
                    {
                        var video = (VideoOnlyStreamInfo)CombineAfterDownloadStreamVideo;
                        return video.Container.Name;
                    }
                    else
                    {
                        return Stream.Container.Name;
                    }
                }
            }

            public Action AfterDownloadAction { get; set; }
        }
    }
}
