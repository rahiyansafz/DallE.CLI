using System.Text.Json.Serialization;

namespace DallE.CLI.Callback;
internal class DallEData
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
