using System.Collections.Generic;
using System.Linq;
using CommandPalette.Core;

namespace CommandPalette.Plugins {
    internal static class PluginManager {
        private static readonly List<IPlugin> plugins = new List<IPlugin>();

        internal static void Register(IPlugin plugin) {
            plugins.Add(plugin);
        }

        internal static bool Unregister(IPlugin plugin) {
            return plugins.Remove(plugin);
        }

        internal static List<ResultEntry> GetResults(Query query) {
            List<ResultEntry> results = new List<ResultEntry>();
            foreach (IPlugin plugin in plugins) {
                if(!plugin.IsValid(query)) {
                    continue;
                }

                results.AddRange(plugin.GetResults(query));
            }

            return results;
        }

        internal static List<ResultEntry> GetResultsSorted(Query query) {
            List<(IPlugin plugin, ResultEntry entry)> results = new List<(IPlugin plugin, ResultEntry entry)>();
            foreach (IPlugin plugin in plugins) {
                if (!plugin.IsValid(query)) {
                    continue;
                }

                results.AddRange(plugin.GetResults(query).Select(entry => (plugin, entry)));
            }

            return results.OrderByDescending(x => x.plugin.PriorityMultiplier * x.entry.Score).Select(x => x.entry).ToList();
        }

        internal static IEnumerable<IPlugin> GetPlugins() {
            return plugins;
        }

        internal static IEnumerable<T> GetPlugins<T>() {
            return plugins.Where(p => p is T).Cast<T>();
        }
    }
}