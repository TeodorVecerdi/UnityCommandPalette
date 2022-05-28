using System;
using UnityEditor;

namespace CommandPalette.Plugins {
    public interface IPluginSettingsProvider {
        Type SettingsType { get; }
        void DrawSettings(SerializedObject settings, string searchContext);
    }
}