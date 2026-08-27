using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WikiLinks;

internal class Wayback
{
    private static readonly HttpClient Client = new();
    private static readonly string LastBetaDate = new DateTime(2025, 11, 14, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime().ToString("yyyyMMdd");
    private static readonly Dictionary<string, string> Cache = [];

    internal static async Task<string> GetWaybackUrl(string url)
    {
        if (Cache.TryGetValue(url, out string cachedResult))
        {
            return cachedResult;
        }

        var apiUrl = $"https://archive.org/wayback/available?url={Uri.EscapeDataString(url)}&timestamp={LastBetaDate}";

        try
        {
            var json = await Client.GetStringAsync(apiUrl);
            var response = JsonConvert.DeserializeObject<WaybackResponse>(json);

            var result = response.ArchivedSnapshots?.Closest?.Available == true ? response.ArchivedSnapshots.Closest.Url : null;
            Cache[url] = result;

            return result;
        }
        catch
        {
            return null;
        }
    }

    internal class ArchivedSnapshots
    {
        [JsonProperty("closest")]
        public ClosestSnapshot Closest { get; set; }
    }

    internal class ClosestSnapshot
    {
        [JsonProperty("available")]
        public bool Available { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    internal class WaybackResponse
    {
        [JsonProperty("archived_snapshots")]
        public ArchivedSnapshots ArchivedSnapshots { get; set; }
    }
}