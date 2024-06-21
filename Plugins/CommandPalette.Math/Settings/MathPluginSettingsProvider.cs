using System.Collections.Generic;
using CommandPalette.Plugins;
using UnityEditor;

namespace CommandPalette.Math {
    public partial class MathPlugin : IPluginSettingsProvider<MathPluginSettings> {
        public void AddKeywords(HashSet<string> keywords) {
            keywords.Add("Decimal Places");
        }

        public void DrawSettings(SerializedObject settings) {
            EditorGUILayout.PropertyField(settings.FindProperty(MathPluginSettings.DISPLAY_DECIMAL_PLACES_PROPERTY));
            EditorGUILayout.PropertyField(settings.FindProperty(MathPluginSettings.COPY_DECIMAL_PLACES_PROPERTY));
            settings.ApplyModifiedProperties();
        }
    }
}