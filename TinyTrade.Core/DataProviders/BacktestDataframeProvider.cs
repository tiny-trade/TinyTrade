using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.IO.Compression;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Statics;
using TinyTrade.Statics;

namespace TinyTrade.Core.DataProviders;

/// <summary>
///   Data provider for backtest data, automatically handles downloading of the data in <see cref="Load(IProgress{TinyTrade.Core.DataProviders.IDataframeProvider.LoadProgress}?)"/>
/// </summary>
public class BacktestDataframeProvider : IDataframeProvider
{
    protected List<DataFrame> frames;
    // Currently using Binance for backtest data
    private const string BaseUrl = "https://data.binance.vision/data/spot/monthly/klines";
    private readonly TimeInterval interval;
    private readonly int granularity;

    private readonly HttpClient httpClient;
    private int currentIndex = 0;

    public int FramesCount => frames.Count;

    public IReadOnlyCollection<DataFrame> Frames => frames;

    public Timeframe Timeframe { get; private set; }

    public Pair Pair { get; private set; }

    internal BacktestDataframeProvider(TimeInterval interval, Pair pair, Timeframe timeframe)
    {
        this.interval = interval;
        Pair = pair;
        Timeframe = timeframe;
        granularity = timeframe;
        frames = new List<DataFrame>();
        httpClient = new HttpClient();
    }


    public virtual async Task Load(IProgress<IDataframeProvider.LoadProgress>? progress = null)
    {
        IDataframeProvider.LoadProgress prog = new IDataframeProvider.LoadProgress
        {
            Description = "Downloading data"
        };
        var valueProgress = new Progress<float>(v =>
        {
            prog.Progress = v;
            progress?.Report(prog);
        });
        progress?.Report(prog);
        await DownloadAndExtractData(valueProgress);
        prog.Description = "Building dataframes";
        frames = await BuildDataFrames(valueProgress);
    }

    public virtual void Reset(Guid? identifier = null) => currentIndex = 0;

    public virtual async Task<DataFrame?> Next(Guid? identifier = null)
    {
        await Task.CompletedTask;
        return currentIndex >= frames.Count ? null : frames[currentIndex++];
    }

    private string GenerateUrlForSingle(Pair pair, string monthDate) => $"{BaseUrl}/{pair.ForBinance()}/1m/{pair.ForBinance()}-1m-{monthDate}.zip";

    private async Task DownloadAndExtractData(IProgress<float>? progress = null)
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

            var toDownload = new List<(string, string)>();

            foreach (var period in periods)
            {
                var fileName = $"{Paths.UserData}/{Pair.ForBinance()}-1m-{period}.csv";
                if (!File.Exists(fileName))
                {
                    var archiveName = $"{Paths.Cache}/{Pair}-{period}.zip";
                    if (!File.Exists(archiveName))
                    {
                        toDownload.Add((GenerateUrlForSingle(Pair, period), archiveName));
                    }
                    archives.Add(archiveName);
                }
            }

            await Parallel.ForEachAsync(toDownload, new ParallelOptions() { MaxDegreeOfParallelism = 16 }, async (p, token) =>
            {
                await httpClient.DownloadFile(p.Item1, p.Item2);
                Interlocked.Increment(ref val);
                progress?.Report((float)val / (toDownload.Count - 1));
            });

            for (var i = 0; i < archives.Count; i++)
            {
                progress?.Report((float)i / (archives.Count - 1));
                ZipFile.ExtractToDirectory(archives[i], Paths.UserData, true);
            }
        });
    }

    private async Task<List<DataFrame>> BuildDataFrames(IProgress<float>? progress = null)
    {
        var frames = new List<DataFrame>();
        await Task.Run(() =>
        {
            var module = 0;
            float moduleO = 0;
            ulong moduleOt = 0;
            float moduleH = 0;
            float moduleL = 0;
            float moduleV = 0;

            var prefix = $"{Paths.UserData}/{Pair.ForBinance()}-1m-";
            var periods = interval.GetPeriods();
            for (int i = 0; i < periods.Count(); i++)
            {
                var p = periods.ElementAt(i);
                var path = prefix + p + ".csv";
                if (!File.Exists(path)) continue;
                using var csvParser = new TextFieldParser(path);
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { ";", "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                while (!csvParser.EndOfData)
                {
                    var fields = csvParser.ReadFields();
                    if (fields is null || fields.Length < 6) continue;

                    var ot = ulong.Parse(fields[0], CultureInfo.InvariantCulture);
                    var o = float.Parse(fields[1], CultureInfo.InvariantCulture);
                    var h = float.Parse(fields[2], CultureInfo.InvariantCulture);
                    var l = float.Parse(fields[3], CultureInfo.InvariantCulture);
                    var c = float.Parse(fields[4], CultureInfo.InvariantCulture);
                    var v = float.Parse(fields[5], CultureInfo.InvariantCulture);
                    var ct = ulong.Parse(fields[6], CultureInfo.InvariantCulture);

                    if (module == 0)
                    {
                        moduleOt = ot;
                        moduleO = o;
                        moduleH = h;
                        moduleL = l;
                        moduleV = v;
                    }
                    else
                    {
                        moduleV += v;
                    }
                    module++;
                    moduleL = moduleL > l ? l : moduleL;
                    moduleH = moduleH < h ? h : moduleH;
                    if (module != 0 && module % granularity == 0)
                    {
                        var frame = new DataFrame(moduleOt, moduleO, moduleH, moduleL, c, moduleV, ct, true);
                        frames.Add(frame);
                        module = 0;
                    }
                    else
                    {
                        var frame = new DataFrame(ot, o, h, l, c, v, ct, false);
                        frames.Add(frame);
                    }
                }
                progress?.Report((float)(i + 1) / periods.Count());
            }
        });

        return frames;
    }
}