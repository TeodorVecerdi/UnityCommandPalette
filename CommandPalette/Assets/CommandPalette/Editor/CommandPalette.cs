﻿using CommandPalette.Plugins;

namespace CommandPalette {
    public static class CommandPalette {
        public static void RegisterPlugin(IPlugin plugin) => PluginManager.Register(plugin);
        public static bool UnregisterPlugin(IPlugin plugin) => PluginManager.Unregister(plugin);
    }
}