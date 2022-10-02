using System.Text.Json;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Services.Data;

internal interface IDataframeConverter
{
    DataFrame ConvertString(JsonElement json);
}