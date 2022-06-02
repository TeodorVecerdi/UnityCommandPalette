using System;
using System.Collections.Generic;
using CommandPalette.Plugins;
using UnityEditor;

namespace CommandPalette.Basic {
    public partial class CommandsPlugin : IPluginSettingsProvider {
        public Type SettingsType { get; } = typeof(CommandsPluginSettings);

        public void AddKeywords(HashSet<string> keywords) {
            keywords.Add("Search Cutoff");
        }

        public void DrawSettings(SerializedObject settings) {
            EditorGUILayout.PropertyField(settings.FindProperty("m_SearchCutoff"));
            settings.ApplyModifiedProperties();
        }
    }
}