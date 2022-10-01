namespace TinyTrade.Services;

internal interface IDataDownloadService
{
    Task DownloadData(string pair, string intervalPattern, string folder);
}