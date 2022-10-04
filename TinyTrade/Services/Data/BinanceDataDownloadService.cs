using System.IO.Compression;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Statics;
using TinyTrade.Statics;

namespace TinyTrade.Services.Data;

internal class BinanceDataDownloadService : IDataDownloadService
{
    private const string BaseUrl = "https://data.binance.vision/data/spot/monthly/klines";
    private readonly HttpClient httpClient;

    public BinanceDataDownloadService()
    {
        httpClient = new HttpClient();
    }

    public async Task DownloadData(string pair, TimeInterval interval, IProgress<(string, float)>? progress = null)
    {
        if (!Directory.Exists(Paths.Cache))
        {
            Directory.CreateDirectory(Paths.Cache);
        }
        var archives = new List<string>();
        await Task.Run(async () =>
        {
            var periods = interval.GetPeriods();
            var val = 0;

            await Parallel.ForEachAsync(periods, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, async (p, token) =>
            {
                var elem = p;
                var fileName = $"{Paths.UserData}/{pair}-1m-{elem}.csv";
                if (!File.Exists(fileName))
                {
                    var archiveName = $"{Paths.Cache}/{pair}-{elem}.zip";
                    if (!File.Exists(archiveName))
                    {
                        var url = GenerateUrlForSingle(pair, elem);
                        await httpClient.DownloadFile(url, archiveName);
                    }
                    archives.Add(archiveName);
                }
                Interlocked.Increment(ref val);
                progress?.Report(("Downloading data", (float)val / (periods.Count() - 1)));
            });

            for (var i = 0; i < archives.Count; i++)
            {
                progress?.Report(("Extracting data", (float)i / (archives.Count - 1)));
                ZipFile.ExtractToDirectory(archives[i], Paths.UserData, true);
            }
        });
    }

    public string GenerateUrlForSingle(string pair, string monthDate) => $"{BaseUrl}/{pair}/1m/{pair}-1m-{monthDate}.zip";
}