using CommandPalette.Plugins;
using UnityEngine;

namespace CommandPalette {
    public static class CommandPalette {
        public static void RegisterPlugin(IPlugin plugin) => PluginManager.Register(plugin);
        public static bool UnregisterPlugin(IPlugin plugin) => PluginManager.Unregister(plugin);
        public static T GetSettings<T>(IPluginSettingsProvider settingsProvider) where T : ScriptableObject => (T)PluginSettingsManager.GetSettings(settingsProvider);
    }
}