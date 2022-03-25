using System;
using CommandPalette.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommandPalette.Colors {
    public class ColorResultEntry : ResultEntry {
        private readonly Color m_Color;

        public ColorResultEntry(Color color, ResultDisplaySettings displaySettings, int score, Func<ResultEntry, bool> onSelect) : base(displaySettings, score, onSelect) {
            m_Color = color;
        }

        public override void PostProcessVisualElement(VisualElement element) {
            // if (s_stylesheet == null) {
            //     s_stylesheet = Resources.Load<StyleSheet>("ColorPlugin/Stylesheets/ColorResultEntry");
            // }
            //
            // if (s_stylesheet != null) {
            //     element.styleSheets.Add(s_stylesheet);
            // }

            VisualElement resultIcon = element.Q<VisualElement>(null, "result-entry-icon");
            resultIcon.AddToClassList("color-icon");
            resultIcon.style.unityBackgroundImageTintColor = m_Color;
        }
    }
}