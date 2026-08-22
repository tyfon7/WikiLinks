using EFT;
using EFT.Quests;
using EFT.UI;
using EFT.Utilities;
using UnityEngine;
using UnityEngine.UI;
using WikiLinks.Utils.Wiki;

namespace WikiLinks.Utils;

public static class ButtonUtils
{
    private static SimpleContextMenuButton? _buttonTemplate;
    
    internal static SimpleContextMenuButton GetOrCreateButton(Quest quest, UIElement owner, Transform parent)
    {
        var button = GetButton(parent);
        if (!button)
        {
            _buttonTemplate ??= ItemUiContext.Instance.ContextMenu.transform.Find("InteractionButtonsContainer/Button Template").GetComponent<SimpleContextMenuButton>();
            
            button = Object.Instantiate(_buttonTemplate, parent);
            button.name = "OpeWikiButton";

            var fitter = button.GetOrAddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        var text = $"{"OPEN".Localized()} WIKI";

        button!.Close(); // otherwise the clicks will pile up
        button.Show(text, text, ResourcesCache.Pop<Sprite>("Characteristics/Icons/Inspect"), () => _ = WikiQuery.OpenWikiPage(quest.Id), () => { });

        owner.AddDisposable(button.Close);

        return button;
    }

    internal static SimpleContextMenuButton? GetButton(Transform parent)
    {
        var existing = parent.Find("OpenWikiButton");
        return existing ? existing.GetComponent<SimpleContextMenuButton>() : null;
    }
}