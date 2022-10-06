using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using TinyTrade.Core.Constructs;
using TinyTrade.Statics;

namespace TinyTrade.Core.DataProviders;

public class BacktestDataframeProvider : IDataframeProvider
{
    private readonly TimeInterval interval;
    private readonly string pair;
    private readonly int granularity;
    private List<DataFrame> frames;
    private int currentIndex = 0;

    public int FramesCount => frames.Count;

    public BacktestDataframeProvider(TimeInterval interval, string pair, int granularity)
    {
        this.interval = interval;
        this.pair = pair;
        this.granularity = granularity;
        frames = new List<DataFrame>();
    }

    public async Task Load() => frames = await BuildDataFrames();

    public async Task<DataFrame?> Next()
    {
        await Task.CompletedTask;
        return currentIndex >= frames.Count ? null : frames[currentIndex++];
    }

    private async Task<List<DataFrame>> BuildDataFrames()
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

            var prefix = $"{Paths.UserData}/{pair}-1m-";
            var periods = interval.GetPeriods();
            foreach (var p in periods)
            {
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
            }
        });
        return frames;
    }
}