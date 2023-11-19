using System;
using CommandPalette.Core;
using CommandPalette.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommandPalette.Math {
    public class MathResultEntry : ResultEntry {
        private static StyleSheet s_stylesheet;

        public MathResultEntry(ResultDisplaySettings displaySettings, int score, Func<ResultEntry, bool> onSelect) : base(displaySettings, score, onSelect, MathPlugin.ResourcePathProvider) {
        }

        public override void PostProcessVisualElement(VisualElement element) {
            if (s_stylesheet == null) {
                s_stylesheet = ResourceLoader.Load<StyleSheet>("StyleSheets/MathResultEntry.uss", MathPlugin.ResourcePathProvider);
            }

            if (s_stylesheet != null) {
                element.styleSheets.Add(s_stylesheet);
            }

            element.Q<VisualElement>(null, "result-entry-icon").AddToClassList("math-icon");
            element.AddToClassList("math-result-entry");
        }
    }
}