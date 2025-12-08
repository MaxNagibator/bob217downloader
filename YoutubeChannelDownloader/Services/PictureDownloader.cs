namespace YoutubeChannelDownloader.Services;

public interface IPictureDownloader
{
    Task Download(string url, string path);
}

public class PictureDownloader(HttpClient httpClient) : IPictureDownloader
{
    public async Task Download(string url, string path)
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
    }
}
