namespace YoutubeChannelDownloader.Models;

/// <summary>
/// Представляет статистику директории.
/// </summary>
/// <param name="Count">Количество файлов в директории.</param>
/// <param name="TotalSize">Общий размер файлов в директории в удобочитаемом формате.</param>
/// <param name="AverageSize">Средний размер файлов в директории в удобочитаемом формате.</param>
public record DirectoryStats(int Count, string TotalSize, string AverageSize);
