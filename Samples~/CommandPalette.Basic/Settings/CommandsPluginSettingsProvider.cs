using System.Collections.Generic;
using CommandPalette.Plugins;
using UnityEditor;

namespace CommandPalette.Basic {
    public partial class CommandsPlugin : IPluginSettingsProvider<CommandsPluginSettings> {
        public void AddKeywords(HashSet<string> keywords) {
            keywords.Add("Search Cutoff");
        }

        public void DrawSettings(SerializedObject settings) {
            EditorGUILayout.PropertyField(settings.FindProperty(CommandsPluginSettings.SEARCH_CUTOFF_PROPERTY));
            settings.ApplyModifiedProperties();
        }
    }
}