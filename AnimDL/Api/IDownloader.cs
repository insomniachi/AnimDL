using System.Collections.Generic;

namespace AnimDL.Api;

public interface IDownloader
{
    Task Download(string url, string folder, string filenameWithExt, IDictionary<string,string>? headers = null);
    bool IsDownloading { get; }
    void Dispose();
}

public interface IDownloadOperation
{
    event EventHandler<double> OnProgress;
    void Cancel();
    Task Start();
}
