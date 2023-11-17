using System;
using System.Collections.Generic;
using System.Linq;
using CommandPalette.Settings;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommandPalette.Plugins {
    internal static class PluginSettingsManager {
        internal static Dictionary<IPluginSettingsProvider, ScriptableObject> Settings { get; } = new();

        internal static ScriptableObject GetOrCreateSettings(IPlugin plugin, Type settingsType) {
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(CommandPaletteSettings.GetSettingsPath());
            if (allAssets.FirstOrDefault(x => x.GetType() == settingsType) is not ScriptableObject settings) {
                Debug.Log($"<b>CommandPalette Settings Manager</b>: Creating settings object for plugin <b>{plugin?.Name ?? settingsType.Name}</b>");
                settings = ScriptableObject.CreateInstance(settingsType);
                settings.name = settingsType.Name;
                AssetDatabase.AddObjectToAsset(settings, CommandPaletteSettings.GetOrCreateSettings());
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static void RegisterSettingsProvider(IPluginSettingsProvider settingsProvider, ScriptableObject settings) {
            Settings[settingsProvider] = settings;
        }

        internal static void CleanupAssets() {
            CommandPaletteSettings mainAsset = CommandPaletteSettings.GetOrCreateSettings();
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(CommandPaletteSettings.GetSettingsPath());
            foreach (Object asset in allAssets) {
                if (asset is not ScriptableObject scriptableObject || asset == mainAsset) {
                    continue;
                }

                if (Settings.ContainsValue(scriptableObject)) {
                    continue;
                }

                AssetDatabase.RemoveObjectFromAsset(scriptableObject);
            }

            AssetDatabase.SaveAssets();
        }

        internal static ScriptableObject GetSettings(IPluginSettingsProvider settingsProvider) {
            if (Settings.TryGetValue(settingsProvider, out ScriptableObject settings)) {
                return settings;
            }

            return null;
        }
    }
}