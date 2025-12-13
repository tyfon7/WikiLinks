using System.Text.RegularExpressions;
using UnityEngine;

namespace WikiLinks;

public static class Url
{
    public static void OpenWiki(string id)
    {
        var locale = Settings.UseLocalizedLinks.Value ? LocaleManagerClass.LocaleManagerClass.String_0 : "en";

        var itemName = LocaleManagerClass.LocaleManagerClass.method_7(id + " Name", locale);
        var wikiName = WikiEncode(itemName);

        var localePath = locale == "en" ? string.Empty : $"{locale}/";

        Application.OpenURL($"https://escapefromtarkov.fandom.com/{localePath}wiki/{wikiName}");
    }

    // This is NOT standard url encoding. This is what the wiki does with names.
    public static string WikiEncode(string input)
    {
        return Regex.Replace(input, "<[^>]+>", string.Empty) // Remove xml-style tags (added by mods like ItemInfo)
            .Replace(' ', '_') // Replace spaces with underscore
            .Replace("#", string.Empty); // Remove # character
    }
}