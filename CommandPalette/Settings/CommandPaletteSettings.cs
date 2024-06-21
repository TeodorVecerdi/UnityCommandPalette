using System.IO;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Settings {
    public class CommandPaletteSettings : ScriptableObject {
        [SerializeField] private bool m_ClearSearchOnSelection = true;

        [SerializeField, Range(1, 8)]
        private int m_BlurDownSample = 2;
        [SerializeField]
        private float m_BlurSize = 0.75f;
        [SerializeField, Range(1, 128)]
        private int m_BlurPasses = 8;
        [SerializeField]
        private Color m_BlurTintColor = Color.black;
        [SerializeField, Range(0.0f, 1.0f)]
        private float m_BlurTintAmount = 0.2f;
        [SerializeField, Range(-1.0f, 2.0f)]
        private float m_Vibrancy = 1.0f;

        [SerializeField, HideInInspector] private bool m_RefreshBlur = false;
        [SerializeField, Min(0.1f), HideInInspector] private float m_RefreshBlurFrequency = 1.0f;

        public bool ClearSearchOnSelection => m_ClearSearchOnSelection;
        public int BlurDownSample => m_BlurDownSample;
        public float BlurSize => m_BlurSize;
        public int BlurPasses => m_BlurPasses;
        public Color BlurTintColor => m_BlurTintColor;
        public float BlurTintAmount => m_BlurTintAmount;
        public float Vibrancy => m_Vibrancy;
        public bool RefreshBlur => m_RefreshBlur;
        public float RefreshBlurFrequency => m_RefreshBlurFrequency;

        internal const string kClearSearchOnSelection = nameof(m_ClearSearchOnSelection);
        internal const string kBlurDownSample = nameof(m_BlurDownSample);
        internal const string kBlurSize = nameof(m_BlurSize);
        internal const string kBlurPasses = nameof(m_BlurPasses);
        internal const string kBlurTintColor = nameof(m_BlurTintColor);
        internal const string kBlurTintAmount = nameof(m_BlurTintAmount);
        internal const string kRefreshBlur = nameof(m_RefreshBlur);
        internal const string kRefreshBlurFrequency = nameof(m_RefreshBlurFrequency);
        internal const string kVibrancy = nameof(m_Vibrancy);

        internal static string GetSettingsPath() {
            return $"{CommandPalettePackageLocator.GetCommandPaletteAssetPath()}/Settings/Settings.asset";
        }

        internal static CommandPaletteSettings GetOrCreateSettings() {
            CommandPaletteSettings settings = AssetDatabase.LoadAssetAtPath<CommandPaletteSettings>(GetSettingsPath());
            if (settings != null) {
                return settings;
            }

            settings = CreateInstance<CommandPaletteSettings>();

            string folderPath = Path.GetDirectoryName(GetSettingsPath());
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath!);
            }

            AssetDatabase.CreateAsset(settings, GetSettingsPath());
            AssetDatabase.SaveAssets();
            return settings;
        }

        internal static SerializedObject GetSerializedSettings() {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}