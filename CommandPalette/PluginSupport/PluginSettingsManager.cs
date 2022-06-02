using System;
using System.Collections.Generic;
using System.Linq;
using CommandPalette.Settings;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommandPalette.Plugins {
    internal static class PluginSettingsManager {
        private static readonly Dictionary<IPluginSettingsProvider, ScriptableObject> s_Settings = new Dictionary<IPluginSettingsProvider, ScriptableObject>();

        internal static Dictionary<IPluginSettingsProvider, ScriptableObject> Settings => s_Settings;

        internal static ScriptableObject GetOrCreateSettings(IPlugin plugin, Type settingsType) {
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(CommandPaletteSettings.SETTINGS_PATH);
            ScriptableObject settings = allAssets.FirstOrDefault(x => x.GetType() == settingsType) as ScriptableObject;
            if (settings is not { }) {
                Debug.Log($"<b>CommandPalette Settings Manager</b>: Creating settings object for plugin <b>{plugin?.Name ?? settingsType.Name}</b>");
                settings = ScriptableObject.CreateInstance(settingsType);
                settings.name = settingsType.Name;
                AssetDatabase.AddObjectToAsset(settings, CommandPaletteSettings.GetOrCreateSettings());
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static void RegisterSettingsProvider(IPluginSettingsProvider settingsProvider, ScriptableObject settings) {
            s_Settings[settingsProvider] = settings;
        }

        internal static void CleanupAssets() {
            CommandPaletteSettings mainAsset = CommandPaletteSettings.GetOrCreateSettings();
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(CommandPaletteSettings.SETTINGS_PATH);
            foreach (Object asset in allAssets) {
                if (asset is not ScriptableObject scriptableObject || asset == mainAsset) {
                    continue;
                }

                if (s_Settings.ContainsValue(scriptableObject)) {
                    continue;
                }

                AssetDatabase.RemoveObjectFromAsset(scriptableObject);
            }

            AssetDatabase.SaveAssets();
        }

        internal static ScriptableObject GetSettings(IPluginSettingsProvider settingsProvider) {
            if (s_Settings.TryGetValue(settingsProvider, out ScriptableObject settings)) {
                return settings;
            }

            return null;
        }
    }
}