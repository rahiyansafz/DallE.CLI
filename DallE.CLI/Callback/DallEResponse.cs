using System.Text.Json.Serialization;

using DallE.CLI.Dall_E;

namespace DallE.CLI.Callback;
internal class DallEResponse
{
    [JsonPropertyName("created")]
    public long Created { get; set; }
    [JsonPropertyName("data")]
    public DallEData[]? Data { get; set; }
}
