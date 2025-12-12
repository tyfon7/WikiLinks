using System.Collections.Generic;
using BepInEx.Configuration;

namespace WikiLinks
{
    public class Settings
    {
        // Categories
        private const string GeneralSection = "1. General";

        // General
        public static ConfigEntry<bool> EnableContextMenu { get; set; }
        public static ConfigEntry<bool> EnableQuestButton { get; set; }
        public static ConfigEntry<bool> UseLocalizedLinks { get; set; }

        public static void Init(ConfigFile config)
        {
            var configEntries = new List<ConfigEntryBase>();

            // General
            configEntries.Add(EnableContextMenu = config.Bind(
                GeneralSection,
                "Enable Context Menu",
                true,
                new ConfigDescription(
                    "Show Open Wiki option in context menu for all items",
                    null,
                    new ConfigurationManagerAttributes { })));

            configEntries.Add(EnableQuestButton = config.Bind(
                GeneralSection,
                "Enable Quest Button",
                true,
                new ConfigDescription(
                    "Show Open Wiki button in quest descriptions",
                    null,
                    new ConfigurationManagerAttributes { })));

            configEntries.Add(UseLocalizedLinks = config.Bind(
                GeneralSection,
                "Use Localized Links",
                false,
                new ConfigDescription(
                    "Links will go to your language's version of the page instead of English. Be warned that many pages are not translated and may not exist in your language.",
                    null,
                    new ConfigurationManagerAttributes { })));

            RecalcOrder(configEntries);
        }
        private static void RecalcOrder(List<ConfigEntryBase> configEntries)
        {
            // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
            int settingOrder = configEntries.Count;
            foreach (var entry in configEntries)
            {
                if (entry.Description.Tags[0] is ConfigurationManagerAttributes attributes)
                {
                    attributes.Order = settingOrder;
                }

                settingOrder--;
            }
        }
    }
}
