using Newtonsoft.Json;

namespace WikiLinks.Utils.WaybackMachine;

public class ClosestSnapshot
{
    [JsonProperty("available")]
    public bool Available { get; set; }

    [JsonProperty("url")]
    public string? Url { get; set; }

    [JsonProperty("timestamp")]
    public string? Timestamp { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }
}