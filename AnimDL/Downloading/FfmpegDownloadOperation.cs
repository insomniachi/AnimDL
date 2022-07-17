using AnimDL.Api;
using Xabe.FFmpeg;

namespace AnimDL.Downloading;

public class FfmpegDownloadOperation : IDownloadOperation
{
    public event EventHandler<double>? OnProgress;
    private readonly IConversion _conversion;
    private double _currentProgress = -1;
    private long _pid = -1;

    public static async Task<FfmpegDownloadOperation> Create(string url, string filePath, IDictionary<string,string>? headers = null)
    {
        IMediaInfo result = await FFmpeg.GetMediaInfo(url);
        var conversion = FFmpeg.Conversions.New().AddStream(result.Streams).SetOutput(filePath);

        if (headers is not null && headers.Count > 0)
        {
            conversion.AddParameter($"-headers {string.Join("\r\n", headers.Select(x => $"{x.Key}:{x.Value}"))}");
        }

        return new FfmpegDownloadOperation(conversion);
    }

    public FfmpegDownloadOperation(IConversion conversion)
    {
        _conversion = conversion;

        _conversion.OnProgress += OnConversionProgress;
    }

    private void OnConversionProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
    {
        if(_currentProgress == args.Percent)
        {
            return;
        }

        if(_pid == -1)
        {
            _pid = args.ProcessId;
        }

        _currentProgress = args.Percent;
        OnProgress?.Invoke(this, _currentProgress);
    }

    public async Task Start()
    {
        await _conversion.Start();
    }

    public void Cancel()
    {
        CliWrap.Cli.Wrap($"taskill /f /pid {_pid}");
    }
}
