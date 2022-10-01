using HandierCli;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using TinyTrade.Statics;

namespace TinyTrade.Services.DataDownload;

internal class BinanceDataDownloadService : IDataDownloadService
{
    private const string BaseUrl = "https://data.binance.vision/data/spot/monthly/klines";
    private readonly HttpClient httpClient;
    private readonly ILogger logger;

    public BinanceDataDownloadService(ILoggerProvider provider)
    {
        httpClient = new HttpClient();
        logger = provider.CreateLogger(string.Empty);
    }

    public async Task DownloadData(string pair, string intervalPattern, string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        if (!Directory.Exists(".temp"))
        {
            Directory.CreateDirectory(".temp");
        }
        var interval = new YearMonthInterval(intervalPattern);
        var progress = new ConsoleProgressBar(50);
        logger.LogInformation("Downloading data");
        //var awaiter = ConsoleAwaiter.Factory().Completed("Data downloaded").Info("Downloading data ").Frames(10, "-", "\\", "|", "/").Build();
        await Task.Run(async () =>
        {
            var periods = interval.Periods();
            for (var i = 0; i < periods.Count(); i++)
            {
                var elem = periods.ElementAt(i);
                var fileName = $"{folder}/{pair}-1m-{elem}.csv";
                if (!File.Exists(fileName))
                {
                    var archiveName = $".temp/{elem}.zip";
                    if (!File.Exists(archiveName))
                    {
                        var url = GenerateUrlForSingle(pair, elem);
                        await httpClient.DownloadFile(url, archiveName);
                    }
                    ZipFile.ExtractToDirectory(archiveName, folder, true);
                }
                progress.Report((float)i / (periods.Count() - 1));
            }
        });
        progress.Dispose();
    }

    public string GenerateUrlForSingle(string pair, string monthDate) => $"{BaseUrl}/{pair}/1m/{pair}-1m-{monthDate}.zip";
}