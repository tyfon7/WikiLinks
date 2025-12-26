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
            var header = __instance.transform.Find("HeaderLine");
            var description = __instance.transform.Find("Center/Scrollview/Content/CenterBlock/DescriptionBlock");
            CreateButton(quest, __instance, new(-20, -header.RectTransform().sizeDelta.y - description.RectTransform().sizeDelta.y - 40));
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

            // There's some delayed shenanigans that resize the description, so wait until that's done to position the button
            __instance.WaitOneFrame(() =>
            {
                var description = __instance.transform.Find("Description");
                CreateButton(quest, __instance, new(-20, -description.RectTransform().sizeDelta.y - 25));
            });
        }
    }

    private static void CreateButton(QuestClass quest, UIElement parent, Vector2 offset)
    {
        SimpleContextMenuButton openWikiButton = null;

        var existing = parent.transform.Find("OpenWikiButton");
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

            openWikiButton = UnityEngine.Object.Instantiate<SimpleContextMenuButton>(ButtonTemplate, parent.transform);
            openWikiButton.name = "OpenWikiButton";

            // This is needed or the inner elements all collapse
            var fitter = openWikiButton.GetOrAddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layout = openWikiButton.GetComponent<LayoutElement>();
            layout.ignoreLayout = true;

            // Position on bottom right
            var rect = openWikiButton.RectTransform();
            rect.pivot = new(1, 1);
            rect.anchorMin = rect.anchorMax = new(1, 1);
            rect.anchoredPosition = offset;
        }

        // I know this isn't how you translate things, but it's good enough
        var text = $"{"OPEN".Localized()} WIKI";
        openWikiButton.Show(text, text, CacheResourcesPopAbstractClass.Pop<Sprite>("Characteristics/Icons/Inspect"), () => Url.OpenWiki(quest.Id), () => { });

        parent.AddDisposable(openWikiButton.Close);
    }
}