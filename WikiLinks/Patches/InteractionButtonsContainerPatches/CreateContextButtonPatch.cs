using System;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace WikiLinks.Patches.InteractionButtonsContainerPatches;

public class CreateContextButtonPatch : ModulePatch
{
    private static readonly string[] Targets = ["Wishlist Template(Clone)", "PinLock Button", "Dispose Template(Clone)"];
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InteractionButtonsContainer),
            nameof(InteractionButtonsContainer.CreateContextButton),
            [
                typeof(string),
                typeof(string),
                typeof(SimpleContextMenuButton),
                typeof(RectTransform),
                typeof(Sprite),
                typeof(Action),
                typeof(Action),
                typeof(bool),
                typeof(bool)
            ]
        );
    }

    [PatchPrefix]
    private static void Prefix(string key, ref bool autoClose)
    {
        if (key.EndsWith("WIKI")) { autoClose = false; }
    }
    
    [PatchPostfix]
    private static void Postfix(string key, SimpleContextMenuButton __result)
    {
        if (!key.EndsWith("WIKI")) { return; }
        
        var parent = __result.Transform.parent;
        var targetIndex = __result.Transform.GetSiblingIndex();

        foreach (var target in Targets)
        {
            var targetButton = parent.Find(target);
            if (!targetButton || !targetButton.gameObject.activeInHierarchy) { continue; }

            targetIndex = targetButton.GetSiblingIndex();
            break;
        }
        
        __result.Transform.SetSiblingIndex(targetIndex);
    }
}