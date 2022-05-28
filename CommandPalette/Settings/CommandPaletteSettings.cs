using System.IO;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Settings {
    public class CommandPaletteSettings : ScriptableObject {
        public const string SETTINGS_PATH = "Assets/Plugins/CommandPalette/Editor Resources/Settings.asset";

        [SerializeField, Range(1, 8)]
        private int m_BlurDownSample = 1;
        [SerializeField]
        private float m_BlurSize = 1.3f;
        [SerializeField, Range(1, 128)]
        private int m_BlurPasses = 32;
        [SerializeField]
        private Color m_BlurTintColor = Color.black;
        [SerializeField, Range(0.0f, 1.0f)]
        private float m_BlurTintAmount = 0.2f;

        [SerializeField, HideInInspector] private bool m_RefreshBlur = false;
        [SerializeField, Min(0.1f), HideInInspector] private float m_RefreshBlurFrequency = 1.0f;

        public int BlurDownSample => m_BlurDownSample;
        public float BlurSize => m_BlurSize;
        public int BlurPasses => m_BlurPasses;
        public Color BlurTintColor => m_BlurTintColor;
        public float BlurTintAmount => m_BlurTintAmount;
        public bool RefreshBlur => m_RefreshBlur;
        public float RefreshBlurFrequency => m_RefreshBlurFrequency;

        internal const string kBlurDownSample = nameof(m_BlurDownSample);
        internal const string kBlurSize = nameof(m_BlurSize);
        internal const string kBlurPasses = nameof(m_BlurPasses);
        internal const string kBlurTintColor = nameof(m_BlurTintColor);
        internal const string kBlurTintAmount = nameof(m_BlurTintAmount);
        internal const string kRefreshBlur = nameof(m_RefreshBlur);
        internal const string kRefreshBlurFrequency = nameof(m_RefreshBlurFrequency);

        internal static CommandPaletteSettings GetOrCreateSettings() {
            CommandPaletteSettings settings = AssetDatabase.LoadAssetAtPath<CommandPaletteSettings>(SETTINGS_PATH);
            if (settings != null) {
                return settings;
            }

            settings = CreateInstance<CommandPaletteSettings>();

            string folderPath = Path.GetDirectoryName(SETTINGS_PATH);
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }
            AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
            AssetDatabase.SaveAssets();

            return settings;
        }

        internal static SerializedObject GetSerializedSettings() {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}