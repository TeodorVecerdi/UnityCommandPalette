using System.Collections.Generic;
using CommandPalette.Plugins;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Settings {
    public static class CommandPaletteSettingsProvider {
        private static readonly GUIStyle s_headerStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 16,
            margin = { bottom = 4 }
        };

        private static readonly GUIStyle s_pluginNameStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 13,
        };

        private static readonly GUIStyle s_pluginHeaderStyle = new GUIStyle() {
            margin = {top = 12},
        };

        private static readonly GUIStyle s_pluginContentsStyle = new GUIStyle() {
            margin = {left = 3, right = 3},
        };

        private static readonly GUIStyle s_sectionStyle = new GUIStyle() {
            margin = new RectOffset(8, 8, 8, 0)
        };

        private static readonly GUIStyle s_boxStyle = new GUIStyle("box") {
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
            HashSet<string> keywords = new HashSet<string> {
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
                guiHandler = searchContext => {
                    SerializedObject settings = CommandPaletteSettings.GetSerializedSettings();
                    DrawBlurSettings(settings);
                    settings.ApplyModifiedProperties();

                    if (PluginSettingsManager.Settings.Count <= 0) {
                        return;
                    }

                    GUILayout.Space(8.0f);
                    GUILayout.BeginVertical("Plugins", s_boxStyle);
                    GUILayout.Space(16.0f);
                    foreach ((IPluginSettingsProvider provider, ScriptableObject pluginSettings) in PluginSettingsManager.Settings) {
                        DrawPluginHeader(provider, pluginSettings);
                        GUILayout.BeginVertical(s_pluginContentsStyle);
                        provider.DrawSettings(new SerializedObject(pluginSettings));
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                },
                keywords = keywords
            };
        }

        private static void DrawBlurSettings(SerializedObject settings) {
            GUILayout.BeginVertical("", s_sectionStyle);
            GUILayout.Label("Blur Settings", s_headerStyle);
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurDownSample));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurSize));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurPasses));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurTintColor));
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kBlurTintAmount));
            GUILayout.Space(4.0f);

            SerializedProperty refreshBlurProperty = settings.FindProperty(CommandPaletteSettings.kRefreshBlur);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(refreshBlurProperty);
            // GUI.enabled = refreshBlurProperty.boolValue;
            EditorGUILayout.PropertyField(settings.FindProperty(CommandPaletteSettings.kRefreshBlurFrequency));
            GUI.enabled = true;
            GUILayout.EndVertical();
        }

        private static void DrawPluginHeader(IPluginSettingsProvider provider, ScriptableObject pluginSettings) {
            string pluginName = provider is IPlugin plugin ? plugin.Name : provider.GetType().Name;
            GUILayout.BeginHorizontal(s_pluginHeaderStyle);
            GUILayout.Label(pluginName, s_pluginNameStyle);
            GUI.enabled = false;
            EditorGUILayout.ObjectField((string)null, pluginSettings, pluginSettings.GetType(), true, GUILayout.ExpandWidth(true), GUILayout.MinWidth(256.0f));
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}