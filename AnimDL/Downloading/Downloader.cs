using AnimDL.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sharprompt;

namespace AnimDL.Downloading;

public class Downloader : IDownloader
{
    private readonly ILogger<Downloader> _logger;
    private readonly IConfiguration _config;
    private ProgressBar? _progressBar;
    private IDownloadOperation? _currentDownload;

    public bool IsDownloading { get; set; }

    public Downloader(ILogger<Downloader> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task Download(string url, string folder, string filenameWithExt, IDictionary<string,string>? headers = null)
    {
        var downloadFolder = _config["DownloadFolder"];
        var specificDownloadFolder = Path.Combine(downloadFolder, folder);
        Directory.CreateDirectory(specificDownloadFolder);
        var filePath = Path.Combine(specificDownloadFolder, filenameWithExt);

        _logger.LogInformation("Starting download for {File}", Path.Combine(folder, filenameWithExt));

        if(File.Exists(filePath))
        {
            _logger.LogWarning("File already exists");

            var answer = Prompt.Confirm("Delete?", defaultValue: true);

            if(answer)
            {
                File.Delete(filePath);
            }
            else
            {
                return;
            }
        }

        // TODO: change this when other downloaders are implemented
        // TODO: choose downloader based on file extension, ffmpeg only needed for hls streams
        _currentDownload = await FfmpegDownloadOperation.Create(url, filePath, headers);
        _currentDownload.OnProgress += Conversion_OnProgress;

        IsDownloading = true;
        _progressBar = new ProgressBar();
        await _currentDownload.Start();
        _progressBar.Dispose();
        IsDownloading = false;
    }

    private void Conversion_OnProgress(object? sender, double progress)
    {
        _progressBar?.Report(progress / 100);
    }

    public void Dispose()
    {
        if(IsDownloading)
        {
            _currentDownload?.Cancel();
        }
    }
}
