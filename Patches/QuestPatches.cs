using System.Reflection;
using EFT.GlobalEvents;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.UI;

namespace WikiLinks;

using DailyQuest = GClass3996;

public static class QuestPatches
{
    private static SimpleContextMenuButton ButtonTemplate;

    public static void Enable()
    {
        new QuestObjectivesViewPatch().Enable();
        new NotesTaskDescriptionPatch().Enable();
    }

    // Works for accepted quests in both trader/tasks and character/tasks
    public class QuestObjectivesViewPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(QuestObjectivesView), nameof(QuestObjectivesView.Show)).MakeGenericMethod([typeof(QuestClass)]);
        }

        [PatchPostfix]
        public static void Postfix(QuestObjectivesView __instance, IConditional conditional)
        {
            if (conditional is not QuestClass quest)
            {
                return;
            }

            if (!Settings.EnableQuestButton.Value || quest is DailyQuest)
            {
                var unwantedButton = GetButton(__instance.transform);
                if (unwantedButton != null)
                {
                    unwantedButton.Close();
                }

                return;
            }

            var button = GetOrCreateButton(quest, __instance, __instance.transform);
            if (button == null)
            {
                return;
            }

            var layout = button.GetComponent<LayoutElement>();
            layout.ignoreLayout = true;

            // Position on right
            var rect = button.RectTransform();
            rect.pivot = new(1, 1);
            rect.anchorMin = rect.anchorMax = new(1, 1);
            rect.anchoredPosition = new(-20, rect.anchoredPosition.y);
        }
    }

    // Traders/task screen for unaccepted quests
    public class NotesTaskDescriptionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(NotesTaskDescription), nameof(NotesTaskDescription.Show));
        }

        [PatchPostfix]
        public static void Postfix(UIElement __instance, QuestClass quest)
        {
            var description = __instance.transform.Find("Center/Scrollview/Content/CenterBlock/DescriptionBlock");

            if (!Settings.EnableQuestButton.Value || quest is DailyQuest || quest.QuestStatus >= EQuestStatus.Started) // started quests are handled above
            {
                var unwantedButton = GetButton(description.parent);
                if (unwantedButton != null)
                {
                    unwantedButton.Close();
                }

                return;
            }

            var button = GetOrCreateButton(quest, __instance, description.parent);
            if (button == null)
            {
                return;
            }

            button.transform.SetSiblingIndex(description.GetSiblingIndex() + 1);

            // Position on right
            var rect = button.RectTransform();
            rect.pivot = new(1, 1);
        }
    }

    private static SimpleContextMenuButton GetButton(Transform parent)
    {
        var existing = parent.Find("OpenWikiButton");
        if (existing != null)
        {
            return existing.GetComponent<SimpleContextMenuButton>();
        }

        return null;
    }

    private static SimpleContextMenuButton GetOrCreateButton(QuestClass quest, UIElement owner, Transform parent)
    {
        SimpleContextMenuButton button = GetButton(parent);
        if (button == null)
        {
            // Find a button to clone
            ButtonTemplate ??= ItemUiContext.Instance.ContextMenu.transform.Find("InteractionButtonsContainer/Button Template")?.GetComponent<SimpleContextMenuButton>();

            button = UnityEngine.Object.Instantiate(ButtonTemplate, parent);
            button.name = "OpenWikiButton";

            // This is needed or the inner elements all collapse
            var fitter = button.GetOrAddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // I know this isn't how you translate things, but it's good enough
        var text = $"{"OPEN".Localized()} WIKI";

        button.Close(); // otherwise the clicks will pile up
        button.Show(text, text, CacheResourcesPopAbstractClass.Pop<Sprite>("Characteristics/Icons/Inspect"), () => Url.OpenWiki(quest.Id), () => { });

        owner.AddDisposable(button.Close);

        return button;
    }
}