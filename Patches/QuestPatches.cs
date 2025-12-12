using System.Linq;
using System.Reflection;
using EFT.Quests;
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
        new QuestObjectivesViewPatch().Enable();
    }

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

            SimpleContextMenuButton openWikiButton = null;

            var existing = __instance.transform.Find("OpenWikiButton");
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

                openWikiButton = UnityEngine.Object.Instantiate<SimpleContextMenuButton>(ButtonTemplate, __instance.transform);
                openWikiButton.name = "OpenWikiButton";
                openWikiButton.transform.SetSiblingIndex(1); // After label so it's on top

                // This is needed or the inner elements all collapse
                var fitter = openWikiButton.GetOrAddComponent<ContentSizeFitter>();
                fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var layout = openWikiButton.GetComponent<LayoutElement>();
                layout.ignoreLayout = true;

                // Position on right
                var rect = openWikiButton.RectTransform();
                rect.pivot = new(1, 1);
                rect.anchorMin = rect.anchorMax = new(1, 1);

                openWikiButton.transform.localPosition = new(openWikiButton.transform.localPosition.x, openWikiButton.transform.localPosition.y - 10, 0);
            }

            // I know this isn't how you translate things, but it's good enough
            var text = $"{"OPEN".Localized()} WIKI";

            openWikiButton.Show(text, text, CacheResourcesPopAbstractClass.Pop<Sprite>("Characteristics/Icons/Inspect"), () =>
            {
                var locale = Settings.UseLocalizedLinks.Value ? LocaleManagerClass.LocaleManagerClass.String_0 : "en";

                var itemName = LocaleManagerClass.LocaleManagerClass.method_7(quest.Id + " Name", locale);
                var wikiName = itemName.Replace(' ', '_');
                var localePath = locale == "en" ? string.Empty : $"{locale}/";

                Application.OpenURL($"https://escapefromtarkov.fandom.com/{localePath}wiki/{wikiName}");
            },
            () => { });

            __instance.AddDisposable(openWikiButton.Close);
        }
    }
}