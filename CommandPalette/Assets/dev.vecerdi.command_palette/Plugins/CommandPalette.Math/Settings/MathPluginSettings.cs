using UnityEngine;

namespace CommandPalette.Math {
    public class MathPluginSettings : ScriptableObject {
        [SerializeField, Range(0, 28), Tooltip("The number of decimal places to display in the result entry.")]
        private int m_DisplayDecimalPlaces = 8;
        [SerializeField, Range(0, 28), Tooltip("The number of decimal places to use when copying the result to the clipboard.")]
        private int m_CopyDecimalPlaces = 28;

        public int DisplayDecimalPlaces => m_DisplayDecimalPlaces;
        public int CopyDecimalPlaces => m_CopyDecimalPlaces;

        internal const string DISPLAY_DECIMAL_PLACES_PROPERTY = nameof(m_DisplayDecimalPlaces);
        internal const string COPY_DECIMAL_PLACES_PROPERTY = nameof(m_CopyDecimalPlaces);
    }
}