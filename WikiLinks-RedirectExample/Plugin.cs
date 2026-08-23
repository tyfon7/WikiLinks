using BepInEx;

namespace WikiLinksRedirectExample;

[BepInDependency("com.tyfon.wikilinks")]
[BepInPlugin( "com.tyfon.wikilinks.redirectexample", "WikiLinksRedirectExample", "1.0.0" )]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Add redirect for Physical Bitcoin to bushtail's website
        WikiLinks.RedirectRegistry.AddWikiRedirect("59faff1d86f7746c51718c9c", "https://bushtail.ca");
    }
}