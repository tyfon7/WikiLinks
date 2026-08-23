using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;

namespace WikiLinks;

[UsedImplicitly]
public class RedirectRegistry
{
    private static readonly Dictionary<MongoID, string> Redirects = new();

    [UsedImplicitly]
    public static bool AddWikiRedirect(string id, string link)
    {
        return Redirects.TryAdd(id, link);
    }

    internal static bool TryGetValue(string id, out string link)
    {
        return Redirects.TryGetValue(id, out link);
    }
}