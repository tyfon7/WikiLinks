using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using WikiLinks.Utils.WaybackMachine;

namespace WikiLinks.Utils.Wiki;

internal static class WikiQuery
{
    private static readonly HttpClient Client = new();
    private static readonly DateTime LastBetaDate = new(2025, 11, 14, 0, 0, 0, DateTimeKind.Utc);

    internal static async Task OpenWikiPage(string id)
    {
        var locale = Plugin.UseLocalizedLinks!.Value ? EFT.LocalizationManager.Instance.Culture : "en";

        if (RedirectRegistry.TryGetValue(id, out var redirect))
        {
            Application.OpenURL(redirect);
            return;
        }

        EFT.LocalizationManager.Instance.TryGetLocalization($"{id} Name", locale, out var localization);

        if (string.IsNullOrEmpty(localization)) { return; }

        var wikiName = EncodeWikiUri(localization);
        var localePath = locale == "en" ? string.Empty : $"{locale}/";
        var baseUrl = $"https://escapefromtarkov.fandom.com/{localePath}wiki/{wikiName}";

        if (Plugin.UseArchiveOrg!.Value)
        {
            var archiveUrl = await GetArchiveUrlBeforeDate(baseUrl, LastBetaDate);
            Application.OpenURL(archiveUrl);
        }
        else
        {
            Application.OpenURL(baseUrl);
        }
    }

    private static async Task<string?> GetArchiveUrlBeforeDate(string targetUrl, DateTime beforeDate)
    {
        var timestamp = beforeDate.ToUniversalTime().ToString("yyyyMMdd");
        var apiUrl = $"https://archive.org/wayback/available?url={Uri.EscapeDataString(targetUrl)}&timestamp={timestamp}";

        try
        {
            var json = await Client.GetStringAsync(apiUrl);
            var result = JsonConvert.DeserializeObject<WaybackResponse>(json);

            return result?.ArchivedSnapshots?.Closest?.Available == true ? result.ArchivedSnapshots.Closest.Url : null;
        }
        catch { return null; }
    }

    private static string EncodeWikiUri(string input)
    {
        return Regex.Replace(input, "<[^>]+>", string.Empty)
            .Replace("[K] ", string.Empty)
            .Replace(' ', '_')
            .Replace("#", string.Empty);
    }
}





