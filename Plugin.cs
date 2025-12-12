using BepInEx;

namespace WikiLinks
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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
