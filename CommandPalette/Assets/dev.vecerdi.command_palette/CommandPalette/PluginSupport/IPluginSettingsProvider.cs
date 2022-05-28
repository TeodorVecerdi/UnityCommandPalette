using System;
using System.Collections.Generic;
using UnityEditor;

namespace CommandPalette.Plugins {
    public interface IPluginSettingsProvider {
        Type SettingsType { get; }
        void DrawSettings(SerializedObject settings);
        void AddKeywords(HashSet<string> keywords);
    }
}