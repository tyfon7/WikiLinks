using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;

namespace WikiLinks;

[UsedImplicitly]
public class RedirectRegistry
{
    private static readonly Dictionary<MongoID, string> Redirects = new();
    
    /// <summary>
    ///     Map a URL to an item template ID.
    /// </summary>
    /// <param name="id">The item template ID.</param>
    /// <param name="link">The URL to redirect to.</param>
    /// <returns>True if the redirect was added, false if it already existed.</returns>
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