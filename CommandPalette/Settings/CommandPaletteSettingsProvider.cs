using System.Collections.Generic;
using CommandPalette.Plugins;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Settings {
    public static class CommandPaletteSettingsProvider {
        private static readonly GUIStyle s_HeaderStyle = new(EditorStyles.boldLabel) {
            fontSize = 16,
            margin = { bottom = 4 }
        };

        private static readonly GUIStyle s_PluginNameStyle = new(EditorStyles.boldLabel) {
            fontSize = 13,
        };

        private static readonly GUIStyle s_PluginHeaderStyle = new() {
            margin = {top = 12},
        };

        private static readonly GUIStyle s_PluginContentsStyle = new() {
            margin = {left = 3, right = 3},
        };

        private static readonly GUIStyle s_SectionStyle = new() {
            margin = new RectOffset(8, 8, 8, 0)
        };

        private static readonly GUIStyle s_BoxStyle = new("box") {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = {
                textColor = new Color(0.752f, 0.752f, 0.752f, 1.0f)
            },
            padding = new RectOffset(8, 8, 8, 8),
        };

        [SettingsProvider]
        public static SettingsProvider CreateProvider() {
            PluginSettingsManager.CleanupAssets();
            HashSet<string> keywords = new() {
                "Command Palette",
                "Blur",
                "Down Sample",
                "Size",
                "Passes",
                "Tint",
                "Color",
                "Amount"
            };

            foreach ((IPluginSettingsProvider provider, _) in PluginSettingsManager.Settings) {
                provider.AddKeywords(keywords);
            }

            return new SettingsProvider("Project/CommandPalette", SettingsScope.Project) {
                label = "Command Palette",
                guiHandler = _ => {
                    SerializedObject settings = CommandPaletteSettings.GetSerializedSettings();
                    DrawBlurSettings(settings);
                    settings.ApplyModifiedProperties();

                    if (PluginSettingsManager.Settings.Count <= 0) {
                        return;
                    }

                    GUILayout.Space(8.0f);
                    GUILayout.BeginVertical("Plugins", s_BoxStyle);
                    GUILayout.Space(24.0f);
                    List<(IPluginSettingsProvider, ScriptableObject)> newSettings = new();
                    foreach ((IPluginSettingsProvider provider, ScriptableObject pluginSettings) in PluginSettingsManager.Settings) {
                        ScriptableObject settingsInstance = pluginSettings;
                        if (settingsInstance == null) {
                            settingsInstance = PluginSettingsManager.GetOrCreateSettings(provider as IPlugin, provider.SettingsType);
                            newSettings.Add((provider, settingsInstance));
                        }

                        DrawPluginHeader(provider, settingsInstance);
                        GUILayout.BeginVertical(s_PluginContentsStyle);
                        provider.DrawSettings(new SerializedObject(settingsInstance));
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();

                    foreach ((IPluginSettingsProvider provider, ScriptableObject pluginSettings) in newSettings) {
                        PluginSettingsManager.RegisterSettingsProvider(provider, pluginSettings);
                    }
                },
                keywords = keywords
            };
        }

        private static void DrawBlurSettings(SerializedObject settings) {
            GUILayout.BeginVertical("", s_SectionStyle);
            GUILayout.Label("General Settings", s_HeaderStyle);
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kClearSearchOnSelection));

            GUILayout.Label("Blur Settings", s_HeaderStyle);
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurDownSample));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurSize));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurPasses));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurTintColor));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurTintAmount));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kVibrancy));
            GUILayout.Space(4.0f);

            /*SerializedProperty refreshBlurProperty = settings.FindProperty(CommandPaletteSettings.kRefreshBlur);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(refreshBlurProperty);
            // GUI.enabled = refreshBlurProperty.boolValue;
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kRefreshBlurFrequency));
            GUI.enabled = true;*/
            GUILayout.EndVertical();
        }

        private static void DrawPluginHeader(IPluginSettingsProvider provider, ScriptableObject pluginSettings) {
            string pluginName = provider is IPlugin plugin ? plugin.Name : provider.GetType().Name;
            GUILayout.BeginHorizontal(s_PluginHeaderStyle);
            GUILayout.Label(pluginName, s_PluginNameStyle, GUILayout.Width(256.0f));
            GUI.enabled = false;
            EditorGUILayout.ObjectField((string)null, pluginSettings, pluginSettings.GetType(), true, GUILayout.ExpandWidth(true), GUILayout.MinWidth(256.0f));
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}