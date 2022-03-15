using System;
using UnityEngine.UIElements;

namespace CommandPalette.Core {
    public class ResultEntry {
        public ResultDisplaySettings DisplaySettings { get; }
        public int Score { get; }
        public Func<ResultEntry, bool> OnSelect { get; }
        public object UserData { get; set; }

        public ResultEntry(ResultDisplaySettings displaySettings, int score, Func<ResultEntry, bool> onSelect) {
            DisplaySettings = displaySettings;
            Score = score;
            OnSelect = onSelect;
        }

        public virtual void PostProcessVisualElement(VisualElement element) {
        }
    }
}