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
                "Amount",
                "Clear",
                "Selection",
                "Search",
            };

            foreach ((var provider, _) in PluginSettingsManager.Settings) {
                provider.AddKeywords(keywords);
            }

            return new SettingsProvider("Project/CommandPalette", SettingsScope.Project) {
                label = "Command Palette",
                guiHandler = _ => {
                    var settings = CommandPaletteSettings.GetSerializedSettings();
                    DrawBlurSettings(settings);
                    settings.ApplyModifiedProperties();

                    if (PluginSettingsManager.Settings.Count <= 0) {
                        return;
                    }

                    GUILayout.Space(8.0f);
                    GUILayout.BeginVertical("Plugins", s_BoxStyle);
                    GUILayout.Space(24.0f);
                    List<(IPluginSettingsProvider, ScriptableObject)> newSettings = new();
                    foreach ((var provider, var pluginSettings) in PluginSettingsManager.Settings) {
                        var settingsInstance = pluginSettings;
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

                    foreach ((var provider, var pluginSettings) in newSettings) {
                        PluginSettingsManager.RegisterSettingsProvider(provider, pluginSettings);
                    }
                },
                keywords = keywords
            };
        }

        private static void DrawBlurSettings(SerializedObject serializedObject) {
            var settings = CommandPaletteSettings.GetOrCreateSettings();

            GUILayout.BeginVertical("", s_SectionStyle);
            GUILayout.Label("General Settings", s_HeaderStyle);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.ClearSearchOnSelectionProperty));

            GUILayout.Label("Blur Settings", s_HeaderStyle);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.DownSamplePassesProperty));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.PassesProperty));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.BlurSizeProperty));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.EnableTintProperty));
            if (settings.EnableTint) {
                using var _ = new GUILayout.HorizontalScope();
                GUILayout.Space(16.0f);
                using var __ = new GUILayout.VerticalScope();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.TintAmountProperty));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.TintProperty));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.EnableVibrancyProperty));
            if (settings.EnableVibrancy) {
                using var _ = new GUILayout.HorizontalScope();
                GUILayout.Space(16.0f);
                using var __ = new GUILayout.VerticalScope();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.VibrancyProperty));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.EnableNoiseProperty));
            if (settings.EnableNoise) {
                using var _ = new GUILayout.HorizontalScope();
                GUILayout.Space(16.0f);
                using var __ = new GUILayout.VerticalScope();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.NoiseTextureProperty));
            }

            GUILayout.EndVertical();
        }

        private static void DrawPluginHeader(IPluginSettingsProvider provider, ScriptableObject pluginSettings) {
            var pluginName = provider is IPlugin plugin ? plugin.Name : provider.GetType().Name;
            GUILayout.BeginHorizontal(s_PluginHeaderStyle);
            GUILayout.Label(pluginName, s_PluginNameStyle, GUILayout.Width(256.0f));
            GUI.enabled = false;
            EditorGUILayout.ObjectField((string?)null, pluginSettings, pluginSettings.GetType(), true, GUILayout.ExpandWidth(true), GUILayout.MinWidth(256.0f));
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}
