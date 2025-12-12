using BepInEx;

namespace WikiLinks
{
    [BepInPlugin("com.tyfon.wikilinks", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Settings.Init(Config);

            ContextMenuPatches.Enable();
            QuestPatches.Enable();
        }
    }
}
