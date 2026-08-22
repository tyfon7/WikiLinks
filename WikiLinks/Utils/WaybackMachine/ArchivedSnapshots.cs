using Newtonsoft.Json;

namespace WikiLinks.Utils.WaybackMachine;

public class ArchivedSnapshots
{
    [JsonProperty("closest")]
    public ClosestSnapshot? Closest { get; set; }
}