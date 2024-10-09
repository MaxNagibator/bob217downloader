﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using YoutubeChannelDownloader.Configurations;

namespace YoutubeChannelDownloader;

public static class SerilogFactory
{
    public static SerilogLoggerFactory Init(IServiceProvider provider)
    {
        IOptions<DownloadOptions> options = provider.GetRequiredService<IOptions<DownloadOptions>>();
        string logPath = Path.Combine(options.Value.VideoFolderPath, "logs", "verbose.log");

        Logger logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(logPath,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}]({SourceContext}) {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Logger = logger;
        return new SerilogLoggerFactory(logger);
    }
}
