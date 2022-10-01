using TinyTrade.Core;

namespace TinyTrade.Services.Data;

internal interface IDataDownloadService
{
    Task DownloadData(string pair, TimeInterval interval, IProgress<(string, float)>? progress = null);
}