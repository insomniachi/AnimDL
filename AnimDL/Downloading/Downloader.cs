using AnimDL.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sharprompt;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;

namespace AnimDL.Downloading;

public class Downloader : IDownloader
{
    private readonly ILogger<Downloader> _logger;
    private readonly IConfiguration _config;
    private double _currentProgress = -1;
    private ProgressBar? _progressBar;

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

        IMediaInfo result = await FFmpeg.GetMediaInfo(url);
        var conversion = FFmpeg.Conversions.New().AddStream(result.Streams).SetOutput(filePath);

        if(headers is not null && headers.Count > 0)
        {
            conversion.AddParameter($"-headers {string.Join("\r\n", headers.Select(x => $"{x.Key}:{x.Value}"))}");
        }

        conversion.OnProgress += Conversion_OnProgress;

        _progressBar = new ProgressBar();
        await conversion.Start();
        _progressBar.Dispose();
    }

    private void Conversion_OnProgress(object sender, ConversionProgressEventArgs args)
    {
        if (_currentProgress == args.Percent)
        {
            return;
        }

        _currentProgress = args.Percent;
        _progressBar?.Report(_currentProgress / 100);
    }
}
