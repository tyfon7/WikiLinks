using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EFT;
using UnityEngine;

namespace WikiLinks;

public static class Url
{
    public static async Task OpenWiki(string id)
    {
        var locale = Settings.UseLocalizedLinks.Value ? LocalizationManager.Instance.Culture : "en";

        LocalizationManager.Instance.TryGetLocalization($"{id} Name", locale, out var itemName);

        var wikiName = WikiEncode(itemName);

        var localePath = locale == "en" ? string.Empty : $"{locale}/";

        var baseUrl = $"https://escapefromtarkov.fandom.com/{localePath}wiki/{wikiName}";

        if (RedirectRegistry.TryGetValue(id, out var redirect))
        {
            Application.OpenURL(redirect);
        }
        else
        {
            if (Settings.UseWayback.Value)
            {
                var archiveUrl = await Wayback.GetWaybackUrl(baseUrl);
                Application.OpenURL(archiveUrl ?? baseUrl);
            }
            else
            {
                Application.OpenURL(baseUrl);
            }
        }
    }

    // This is NOT standard url encoding. This is what the wiki does with names.
    public static string WikiEncode(string input)
    {
        var sanitized = Regex.Replace(input, "<[^>]+>", string.Empty) // Remove xml-style tags (added by mods like ItemInfo)
            .Replace("[K] ", string.Empty) // KappaMarker mod
            .Replace(" [PVE ZONE]", string.Empty) // Ref Friendly Quests mod
            .Replace(' ', '_') // Replace spaces with underscore
            .Replace("#", string.Empty); // Remove # character

        return Uri.EscapeDataString(sanitized); // Encode remaining special characters (e.g. '?' -> %3F)
    }
}