namespace TinyTrade.Core.Statics;

public static class Extensions
{
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
}