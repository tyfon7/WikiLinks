using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.Utilities;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using WikiLinks.Utils.Wiki;

namespace WikiLinks.Patches.ItemUiContextPatches;

public class GetItemContextInteractionsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.GetItemContextInteractions));   
    }

    [PatchPostfix]
    private static void Postfix(ItemContext itemContext, ContextInteractions<EItemInfoButton> __result)
    {
        if (!Plugin.EnableContextMenu!.Value)
        {
            return;
        }

        if (__result.GetType().FullName == "UIFixes.EmptySlotMenu")
        {
            return;
        }
        
        var item = itemContext.Item;
        if (item == null)
        {
            return;
        }

        var text = $"{"OPEN".Localized()} WIKI";
        __result._dynamicInteractions["OPEN WIKI"] = new DynamicContextInteraction("OPEN WIKI", text,
            () => _ = WikiQuery.OpenWikiPage(item.TemplateId), ResourcesCache.Pop<Sprite>("Characteristics/Icons/Inspect"));
    }
}