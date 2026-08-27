# Creating your own redirects

Using a simple client mod, you can redirect any item ID's page to your own wiki/website page.

### Example

```csharp
using BepInEx;
using WikiLinks;

[BepInDependency( "com.tyfon.wikilinks")]
[BepInPlugin( "com.tyfon.wikilinks.redirectexample", "WikiLinks Redirect Example", "1.0.0" )]
public class Plugin : BaseUnityPlugin
{
    internal void Awake()
    {
        // Redirects the Physical bitcoin page to the WikiLinks repository.
        WikiLinks.RedirectRegistry.AddWikiRedirect("59faff1d86f7746c51718c9c", "https://github.com/tyfon7/WikiLinks");
    }
}
```