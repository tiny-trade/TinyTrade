using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System.Text;

namespace TinyTrade.Core.Statics;

public static class SerializationHandler
{
    private static JsonSerializer? serializer = null;

    public static T? Deserialize<T>(string parsed)
    {
        using JsonTextReader reader = new JsonTextReader(new StringReader(parsed));
        return ResolverSerializer().Deserialize<T>(reader);
    }

    public static string Serialize<T>(T obj)
    {
        var serializer = ResolverSerializer();
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
        using JsonTextWriter jsonWriter = new JsonTextWriter(sw);
        jsonWriter.Formatting = serializer.Formatting;

        serializer.Serialize(jsonWriter, obj);
        return sw.ToString();
    }

    private static JsonSerializer ResolverSerializer()
    {
        if (serializer is null)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include
            };
            serializer = JsonSerializer.CreateDefault(settings);
            serializer.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
        }
        return serializer;
    }
}