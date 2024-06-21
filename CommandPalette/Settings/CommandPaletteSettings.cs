using System.IO;
using CommandPalette.Utils;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Settings {
    public class CommandPaletteSettings : ScriptableObject {
        [SerializeField] private bool m_ClearSearchOnSelection = true;

        [SerializeField, Range(0, 8)]
        private int m_DownSamplePasses = 3;
        [SerializeField, Range(0, 256)]
        private int m_Passes = 8;
        [SerializeField]
        private float m_BlurSize = 1.0f;

        [SerializeField]
        private bool m_EnableTint;
        [SerializeField]
        private float m_TintAmount;
        [SerializeField]
        private Color m_Tint = Color.black;

        [SerializeField]
        private bool m_EnableVibrancy;
        [SerializeField]
        private float m_Vibrancy;

        [SerializeField]
        private bool m_EnableNoise;
        [SerializeField]
        private Texture2D? m_NoiseTexture;

        public bool ClearSearchOnSelection => m_ClearSearchOnSelection;
        public int DownSamplePasses => m_DownSamplePasses;
        public int Passes => m_Passes;
        public float BlurSize => m_BlurSize;
        public bool EnableTint => m_EnableTint;
        public float TintAmount => m_TintAmount;
        public Color Tint => m_Tint;
        public bool EnableVibrancy => m_EnableVibrancy;
        public float Vibrancy => m_Vibrancy;
        public bool EnableNoise => m_EnableNoise;
        public Texture2D? NoiseTexture => m_NoiseTexture;

        internal const string ClearSearchOnSelectionProperty = nameof(m_ClearSearchOnSelection);
        internal const string DownSamplePassesProperty = nameof(m_DownSamplePasses);
        internal const string PassesProperty = nameof(m_Passes);
        internal const string BlurSizeProperty = nameof(m_BlurSize);
        internal const string EnableTintProperty = nameof(m_EnableTint);
        internal const string TintAmountProperty = nameof(m_TintAmount);
        internal const string TintProperty = nameof(m_Tint);
        internal const string EnableVibrancyProperty = nameof(m_EnableVibrancy);
        internal const string VibrancyProperty = nameof(m_Vibrancy);
        internal const string EnableNoiseProperty = nameof(m_EnableNoise);
        internal const string NoiseTextureProperty = nameof(m_NoiseTexture);

        internal static string GetSettingsPath() {
            return $"{CommandPalettePackageLocator.GetCommandPaletteAssetPath()}/Settings/Settings.asset";
        }

        internal static CommandPaletteSettings GetOrCreateSettings() {
            var settings = AssetDatabase.LoadAssetAtPath<CommandPaletteSettings>(GetSettingsPath());
            if (settings != null) {
                return settings;
            }

            settings = CreateInstance<CommandPaletteSettings>();

            var folderPath = Path.GetDirectoryName(GetSettingsPath());
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

        private void Reset() {
            m_NoiseTexture = ResourceLoader.Load<Texture2D>("Textures/BlueNoise/LDR_RGBA_4.png");
        }
    }
}
