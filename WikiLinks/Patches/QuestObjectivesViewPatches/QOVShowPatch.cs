using System.Reflection;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.UI;
using WikiLinks.Utils;

namespace WikiLinks.Patches.QuestObjectivesViewPatches;

public class QOVShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.DeclaredMethod(typeof(QuestObjectivesView), nameof(QuestObjectivesView.Show))
            .MakeGenericMethod(typeof(Quest));
    }

    [PatchPostfix]
    internal static void Postfix(QuestObjectivesView __instance, IConditional conditional)
    {
        if (conditional is not Quest quest) { return; }

        if (!Plugin.EnableQuestButton!.Value || quest is DailyQuest)
        {
            var unwantedButton = ButtonUtils.GetButton(__instance.Transform);
            if (unwantedButton)
            {
                unwantedButton!.Close();
            }

            return;
        }

        var button = ButtonUtils.GetOrCreateButton(quest, __instance, __instance.Transform);
        if (!button) { return; }
        
        var layout = button.GetComponent<LayoutElement>();
        layout.ignoreLayout = true;
        
        var rect = button.RectTransform();
        rect.pivot = new Vector2(1, 1);
        rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-20, rect.anchoredPosition.y);
    }
}