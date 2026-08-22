using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using JetBrains.Annotations;
using WikiLinks.Patches.InteractionButtonsContainerPatches;
using WikiLinks.Patches.ItemUiContextPatches;
using WikiLinks.Patches.NotesTaskDescriptionPatches;
using WikiLinks.Patches.QuestObjectivesViewPatches;

namespace WikiLinks;

[BepInPlugin("com.tyfon.wikilinks", "WikiLinks", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static readonly Dictionary<MongoID, string> Redirects = new();
    
    public static ConfigEntry<bool>? EnableContextMenu;
    public static ConfigEntry<bool>? EnableQuestButton;
    public static ConfigEntry<bool>? UseLocalizedLinks;
    public static ConfigEntry<bool>? UseArchiveOrg;
    
    private void Awake()
    {
        EnableContextMenu = Config.Bind(
            "1. General",
            "Enable Context Menu",
            true,
            new ConfigDescription("Show Open Wiki option in context menu for all items"));
        
        EnableQuestButton = Config.Bind(
            "1. General",
            "Enable Quest Button",
            true,
            new ConfigDescription("Show Open Wiki button in quest descriptions"));

        UseLocalizedLinks = Config.Bind(
            "1. General",
            "Use Localized Links",
            false,
            new ConfigDescription("Links will go to your language's version of the page instead of English. Be warned that many pages are not translated and may not exist in your language."));

        UseArchiveOrg = Config.Bind(
            "1. General",
            "Use Archive.org",
            false,
            new ConfigDescription("Use Archive.org to attempt to get the last version of the page before 1.0 was released. Because this queries the Wayback Machine, it may take a second for the page to open."));
        
        new GetItemContextInteractionsPatch().Enable();
        new CreateContextButtonPatch().Enable();
        new QOVShowPatch().Enable();
        new NTDShowPatch().Enable();
    }

    [UsedImplicitly]
    public bool AddWikiRedirect(string id, string link)
    {
        return Redirects.TryAdd(id, link);
    }
}