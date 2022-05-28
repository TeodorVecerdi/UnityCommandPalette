using System;
using System.Collections.Generic;
using CommandPalette.Settings;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommandPalette.Plugins {
    internal static class PluginSettingsManager {
        private static readonly Dictionary<IPluginSettingsProvider, ScriptableObject> s_Settings = new Dictionary<IPluginSettingsProvider, ScriptableObject>();

        internal static Dictionary<IPluginSettingsProvider, ScriptableObject> Settings => s_Settings;

        internal static ScriptableObject GetOrCreateSettings(Type settingsType) {
            ScriptableObject settings = AssetDatabase.LoadAssetAtPath(CommandPaletteSettings.SETTINGS_PATH, settingsType) as ScriptableObject;
            if (settings is not { }) {
                settings = ScriptableObject.CreateInstance(settingsType);
                settings.name = settingsType.Name;
                AssetDatabase.AddObjectToAsset(settings, CommandPaletteSettings.GetOrCreateSettings());
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static void RegisterSettingsProvider(IPluginSettingsProvider settingsProvider, ScriptableObject settings) {
            s_Settings.Add(settingsProvider, settings);
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