using System.Reflection;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using WikiLinks.Utils;

namespace WikiLinks.Patches.NotesTaskDescriptionPatches;

public class NTDShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.DeclaredMethod(typeof(NotesTaskDescription), nameof(NotesTaskDescription.Show));
    }

    [PatchPostfix]
    internal static void Postfix(UIElement __instance, Quest quest)
    {
        var description = __instance.transform.Find("Center/Scrollview/Content/CenterBlock/DescriptionBlock");

        if (!Plugin.EnableQuestButton!.Value || quest is DailyQuest || quest.QuestStatus >= EQuestStatus.Started)
        {
            var unwantedButton = ButtonUtils.GetButton(description.parent);

            if (unwantedButton)
            {
                unwantedButton!.Close();
            }

            return;
        }
        
        var button = ButtonUtils.GetOrCreateButton(quest, __instance, description.parent);
        if (!button) { return; }
        
        button.transform.SetSiblingIndex(description.GetSiblingIndex() + 1);

        var rect = button.RectTransform();
        rect.pivot = new Vector2(1, 1);
    }
}