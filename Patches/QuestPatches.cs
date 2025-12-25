using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.UI;

namespace WikiLinks;

public static class QuestPatches
{
    private static SimpleContextMenuButton ButtonTemplate;

    public static void Enable()
    {
        new NotesTaskDescriptionPatch().Enable();
        new NotesTaskDescriptionShortPatch().Enable();
    }

    public class NotesTaskDescriptionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(NotesTaskDescription), nameof(NotesTaskDescription.Show));
        }

        [PatchPostfix]
        public static void Postfix(UIElement __instance, QuestClass quest)
        {
            var parent = __instance.transform.Find("Center/Scrollview/Content/CenterBlock/DescriptionBlock");
            CreateButton(quest, __instance, parent);
        }
    }

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

            var parent = __instance.transform.Find("Description");
            CreateButton(quest, __instance, parent);
        }
    }

    private static void CreateButton(QuestClass quest, UIElement element, Transform parent)
    {
        SimpleContextMenuButton openWikiButton = null;

        var existing = parent.Find("OpenWikiButton");
        if (existing != null)
        {
            openWikiButton = existing.GetComponent<SimpleContextMenuButton>();
        }

        if (!Settings.EnableQuestButton.Value)
        {
            if (openWikiButton != null)
            {
                openWikiButton.Dispose();
                Object.Destroy(openWikiButton);
            }

            return;
        }

        if (openWikiButton == null)
        {
            // Find a button to clone
            ButtonTemplate ??= ItemUiContext.Instance.ContextMenu.transform.Find("InteractionButtonsContainer/Button Template")?.GetComponent<SimpleContextMenuButton>();

            openWikiButton = UnityEngine.Object.Instantiate<SimpleContextMenuButton>(ButtonTemplate, parent);
            openWikiButton.name = "OpenWikiButton";

            // This is needed or the inner elements all collapse
            var fitter = openWikiButton.GetOrAddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layout = openWikiButton.GetComponent<LayoutElement>();
            layout.ignoreLayout = true;

            // Position on bottom right
            var rect = openWikiButton.RectTransform();
            rect.pivot = new(1, 0);
            rect.anchorMin = rect.anchorMax = new(1, 0);
            rect.anchoredPosition = new(-20, -30);
        }

        // I know this isn't how you translate things, but it's good enough
        var text = $"{"OPEN".Localized()} WIKI";
        openWikiButton.Show(text, text, CacheResourcesPopAbstractClass.Pop<Sprite>("Characteristics/Icons/Inspect"), () => Url.OpenWiki(quest.Id), () => { });

        element.AddDisposable(openWikiButton.Close);
    }
}