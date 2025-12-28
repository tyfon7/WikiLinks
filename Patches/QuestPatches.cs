using System.Reflection;
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
        new NotesTaskDescriptionPatch().Enable();
        new NotesTaskDescriptionShortPatch().Enable();
    }

    // Traders/task screen
    public class NotesTaskDescriptionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(NotesTaskDescription), nameof(NotesTaskDescription.Show));
        }

        [PatchPostfix]
        public static void Postfix(UIElement __instance, QuestClass quest)
        {
            var header = __instance.transform.Find("HeaderLine");
            var description = __instance.transform.Find("Center/Scrollview/Content/CenterBlock/DescriptionBlock");
            var button = CreateButton(quest, __instance, __instance.transform);

            if (button == null)
            {
                return;
            }

            // Position on bottom right
            var rect = button.RectTransform();
            rect.pivot = new(1, 1);
            rect.anchorMin = rect.anchorMax = new(1, 1);
            rect.anchoredPosition = new(-20, -header.RectTransform().sizeDelta.y - description.RectTransform().sizeDelta.y - 40);
        }
    }

    // Character/Tasks screen
    public class NotesTaskDescriptionShortPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(NotesTaskDescriptionShort), nameof(NotesTaskDescriptionShort.Show)).MakeGenericMethod([typeof(QuestClass), typeof(AbstractQuestControllerClass)]);
        }

        [PatchPostfix]
        public static void Postfix(UIElement __instance, object conditional)
        {
            if (conditional is not QuestClass quest)
            {
                return;
            }

            var description = __instance.transform.Find("ObjectivesBlock");
            var button = CreateButton(quest, __instance, description);

            if (button == null)
            {
                return;
            }

            // Position on top right of objectives
            var rect = button.RectTransform();
            rect.pivot = new(1, 1);
            rect.anchorMin = rect.anchorMax = new(1, 1);
            rect.anchoredPosition = new(-20, 0);
        }
    }

    private static SimpleContextMenuButton CreateButton(QuestClass quest, UIElement owner, Transform parent)
    {
        SimpleContextMenuButton openWikiButton = null;

        var existing = owner.transform.Find("OpenWikiButton");
        if (existing != null)
        {
            openWikiButton = existing.GetComponent<SimpleContextMenuButton>();
        }

        if (!Settings.EnableQuestButton.Value || quest is DailyQuest)
        {
            openWikiButton.Close();
            return null;
        }

        if (openWikiButton == null)
        {
            // Find a button to clone
            ButtonTemplate ??= ItemUiContext.Instance.ContextMenu.transform.Find("InteractionButtonsContainer/Button Template")?.GetComponent<SimpleContextMenuButton>();

            openWikiButton = UnityEngine.Object.Instantiate(ButtonTemplate, parent);
            openWikiButton.name = "OpenWikiButton";

            // This is needed or the inner elements all collapse
            var fitter = openWikiButton.GetOrAddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layout = openWikiButton.GetComponent<LayoutElement>();
            layout.ignoreLayout = true;
        }

        // I know this isn't how you translate things, but it's good enough
        var text = $"{"OPEN".Localized()} WIKI";

        openWikiButton.Close(); // otherwise the clicks will pile up
        openWikiButton.Show(text, text, CacheResourcesPopAbstractClass.Pop<Sprite>("Characteristics/Icons/Inspect"), () => Url.OpenWiki(quest.Id), () => { });

        owner.AddDisposable(openWikiButton.Close);

        return openWikiButton;
    }
}