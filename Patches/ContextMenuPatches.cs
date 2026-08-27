using System;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.Utilities;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace WikiLinks;

public static class ContextMenuPatches
{
    private static readonly string[] Targets = ["Wishlist Template(Clone)", "PinLock Button", "Dispose Template(Clone)"];

    public static void Enable()
    {
        new AddWikiButtonPatch().Enable();
        new PositionWikiButtonPatch().Enable();
    }

    public class AddWikiButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.GetItemContextInteractions));
        }

        [PatchPostfix]
        private static void Prefix(ItemContext itemContext, ContextInteractions<EItemInfoButton> __result)
        {
            if (!Settings.EnableContextMenu.Value)
            {
                return;
            }

            // Not you, UI Fixes!
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
            __result._dynamicInteractions["OPEN WIKI"] = new DynamicContextInteraction("OPEN WIKI", text, () => _ = Url.OpenWiki(item.TemplateId), ResourcesCache.Pop<Sprite>("Characteristics/Icons/Inspect"));
        }
    }

    public class PositionWikiButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(InteractionButtonsContainer),
                nameof(InteractionButtonsContainer.CreateContextButton),
                [typeof(string), typeof(string), typeof(SimpleContextMenuButton), typeof(RectTransform), typeof(Sprite), typeof(Action), typeof(Action), typeof(bool), typeof(bool)]);
        }

        [PatchPrefix]
        public static void Prefix(string key, ref bool autoClose)
        {
            // Because dumb, dynamic actions are hard-coded to set auto-close to true
            if (key.EndsWith("WIKI"))
            {
                autoClose = false;
            }
        }

        [PatchPostfix]
        public static void Postfix(string key, SimpleContextMenuButton __result)
        {
            if (!key.EndsWith("WIKI"))
            {
                return;
            }

            var parent = __result.Transform.parent;
            var targetIndex = __result.Transform.GetSiblingIndex();

            foreach (var targetName in Targets)
            {
                var targetButton = parent.Find(targetName);
                if (!targetButton || !targetButton.gameObject.activeInHierarchy) { continue; }

                targetIndex = targetButton.GetSiblingIndex();
                break;
            }

            __result.Transform.SetSiblingIndex(targetIndex);
        }
    }
}