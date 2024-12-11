using System.IO;
using Microsoft.AspNetCore.Mvc;
using YoutubeDownloader.ApiModels;
using YoutubeDownloader.Logic;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {
        private DownloadManager _downloadManager;
        private ILogger<MainController> _logger;

        public MainController(DownloadManager downloadManager, ILogger<MainController> logger)
        {
            _downloadManager = downloadManager;
            _logger = logger;
        }

        [HttpPost("AddToDownload")]
        public async Task<IActionResult> AddToDownload(Request model)
        {
            try
            {
                var item = await _downloadManager.AddToQueueAsync(model.Url);
                StateModel stateModel = GetStateModel(item);
                return new JsonResult(stateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddToDownload error");
                // todo сделать мидлварку для ошибок
                return new JsonResult(new { error = true, message = "Всё упало" });
            }
        }

        [HttpGet("state/{id}")]
        public IActionResult State(Guid id)
        {
            var item = _downloadManager.Items.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                return new JsonResult(new { state = "NotFound" });
            }

            StateModel model = GetStateModel(item);
            return new JsonResult(model);
        }

        private static StateModel GetStateModel(DownloadManager.DownloadItem? item)
        {
            var title = item.Video.Title;
            var duration = item.Video.Duration;

            var model = new StateModel
            {
                DownloadId = item.Id,
                Title = title,
                Streams = item.Streams.Select(x => new StateModel.StreamModel
                {
                    Id = x.Id,
                    State = x.State.ToString(),
                    Title = x.Title,
                }).ToArray(),
            };
            return model;
        }

        [HttpGet("download/{id}/{streamId}")]
        public IActionResult Download(Guid id, int streamId)
        {
            var item = _downloadManager.Items.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                return new JsonResult(new { error = true, message = "Фаил не найден" });
            }
            var stream = item.Streams.FirstOrDefault(x => x.Id == streamId);
            if (stream == null)
            {
                return new JsonResult(new { error = true, message = "Фаил не найден" });
            }

            if (stream.State != DownloadItemState.Ready)
            {
                return new JsonResult(new { error = true, message = "Состояние не готово. Текущие " + stream.State });
            }

            var type = stream.VideoType;
            return File(System.IO.File.ReadAllBytes(stream.FullPath), "video/" + type, item.Video.Title + "." + type);
        }

        [HttpGet("SetToDownloadState/{id}/{streamId}")]
        public IActionResult SetToDownloadState(Guid id, int streamId)
        {
            try
            {
                _downloadManager.SetStreamToDownload(id, streamId);
                return new JsonResult(new { message = "Всё оки" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetToDownloadState error");
                // todo сделать мидлварку для ошибок
                return new JsonResult(new { error = true, message = "Всё упало" });
            }
        }

        public class Request
        {
            public string Url { get; set; }
        }
    }
}