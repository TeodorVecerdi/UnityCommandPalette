using System.Collections.Generic;
using System.Linq;
using CommandPalette.Core;

namespace CommandPalette.Plugins {
    internal static class PluginManager {
        private static readonly List<IPlugin> s_Plugins = new();

        internal static void Register(IPlugin plugin) {
            s_Plugins.Add(plugin);

            if (plugin is IPluginSettingsProvider settingsProvider) {
                var settings = PluginSettingsManager.GetOrCreateSettings(plugin, settingsProvider.SettingsType);
                PluginSettingsManager.RegisterSettingsProvider(settingsProvider, settings);
            }
        }

        internal static bool Unregister(IPlugin plugin) {
            return s_Plugins.Remove(plugin);
        }

        internal static List<ResultEntry> GetResults(Query query) {
            var validPlugins = s_Plugins.Where(plugin => plugin.IsValid(query));
            var results = validPlugins.SelectMany(plugin => plugin.GetResults(query));
            return results.ToList();
        }

        internal static List<ResultEntry> GetResultsSorted(Query query) {
            var validPlugins = s_Plugins.Where(plugin => plugin.IsValid(query));
            var results = validPlugins.SelectMany(plugin => plugin.GetResults(query).Select(entry => (Plugin: plugin, Entry: entry)));
            return results.OrderByDescending(x => x.Plugin.PriorityMultiplier * x.Entry.Score)
                          .Select(x => x.Entry)
                          .ToList();
        }

        internal static IEnumerable<IPlugin> GetPlugins() => s_Plugins;
        internal static IEnumerable<T> GetPlugins<T>() => s_Plugins.OfType<T>();
    }
}