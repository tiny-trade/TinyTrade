using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Statics;

public static class Extensions
{
    /// <summary>
    ///   Download a file with an <see cref="HttpClient"/>
    /// </summary>
    /// <returns> </returns>
    public static async Task<bool> DownloadFile(this HttpClient client, string address, string fileName)
    {
        using var response = await client.GetAsync(address);
        if (response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var file = File.OpenWrite(fileName);
            stream.CopyTo(file);
            return true;
        }
        return false;
    }

    /// <summary>
    ///   Attempt to retrieve the value corresponding to the provided key. Return the default value on error
    /// </summary>
    /// <returns> </returns>
    public static T TraitValueOrDefault<T>(this List<Trait> genes, string key, T defaultVal) where T : notnull
    {
        var g = genes.FirstOrDefault(g => g.Key.Equals(key));
        if (g is not null)
        {
            T? res = (T)Convert.ChangeType(g.Value, typeof(T))!;
            return res is null ? defaultVal : (T)res;
        }
        return defaultVal;
    }
}