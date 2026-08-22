using Newtonsoft.Json;

namespace WikiLinks.Utils.WaybackMachine;

public class WaybackResponse
{
    [JsonProperty("archived_snapshots")]
    public ArchivedSnapshots? ArchivedSnapshots { get; set; }
}