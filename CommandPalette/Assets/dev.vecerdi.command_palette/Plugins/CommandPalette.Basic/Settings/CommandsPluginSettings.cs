using UnityEngine;

namespace CommandPalette.Basic {
    public class CommandsPluginSettings : ScriptableObject {
        [SerializeField, Range(0, 100)] private int m_SearchCutoff = 80;

        public int SearchCutoff => m_SearchCutoff;

        internal const string SEARCH_CUTOFF_PROPERTY = nameof(m_SearchCutoff);
    }
}