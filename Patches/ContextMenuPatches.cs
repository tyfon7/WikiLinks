using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
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
        private static void Prefix(ItemContextAbstractClass itemContext, ItemInfoInteractionsAbstractClass<EItemInfoButton> __result)
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

            __result.Dictionary_0["OPEN WIKI"] = new("OPEN WIKI", text, () =>
            {
                var locale = Settings.UseLocalizedLinks.Value ? LocaleManagerClass.LocaleManagerClass.String_0 : "en";

                var itemName = LocaleManagerClass.LocaleManagerClass.method_7(item.TemplateId + " Name", locale);
                var wikiName = itemName.Replace(' ', '_');
                var localePath = locale == "en" ? string.Empty : $"{locale}/";

                Application.OpenURL($"https://escapefromtarkov.fandom.com/{localePath}wiki/{wikiName}");
            },
            CacheResourcesPopAbstractClass.Pop<Sprite>("Characteristics/Icons/Inspect"));
        }
    }

    public class PositionWikiButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionButtonsContainer), "method_1");
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
                if (targetButton != null && targetButton.gameObject.activeInHierarchy)
                {
                    targetIndex = targetButton.GetSiblingIndex();
                    break;
                }
            }

            __result.Transform.SetSiblingIndex(targetIndex);
        }
    }
}