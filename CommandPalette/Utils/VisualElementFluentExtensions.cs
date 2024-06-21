using System;
using UnityEngine.UIElements;

namespace CommandPalette.Utils {
    public static class VisualElementFluentExtensions {
        public static T WithClasses<T>(this T element, params string[] classes) where T : VisualElement {
            foreach (string c in classes) {
                element.AddToClassList(c);
            }
            return element;
        }

        public static T WithClassEnabled<T>(this T element, string className, bool enabled) where T : VisualElement {
            element.EnableInClassList(className, enabled);
            return element;
        }

        public static T WithName<T>(this T element, string name) where T : VisualElement {
            element.name = name;
            return element;
        }

        public static T WithTooltip<T>(this T element, string tooltip) where T : VisualElement {
            element.tooltip = tooltip;
            return element;
        }

        public static T WithChildren<T>(this T element, params VisualElement[] children) where T : VisualElement {
            foreach (VisualElement child in children) {
                element.Add(child);
            }
            return element;
        }

        public static T WithStylesheet<T>(this T element, StyleSheet styleSheet) where T : VisualElement {
            if (styleSheet != null) {
                element.styleSheets.Add(styleSheet);
            }
            return element;
        }

        public static T WithUserData<T>(this T element, object userData) where T : VisualElement {
            element.userData = userData;
            return element;
        }

        public static T AssignedTo<T>(this T element, ref T target) where T : VisualElement {
            target = element;
            return element;
        }

        public static T Enabled<T>(this T element, bool enabled = true) where T : VisualElement {
            element.SetEnabled(enabled);
            return element;
        }

        public static T Disabled<T>(this T element) where T : VisualElement {
            element.SetEnabled(false);
            return element;
        }

        public static T Initialized<T>(this T element, Action<T> action) where T : VisualElement {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(element);
            return element;
        }

        public static T WithText<T>(this T element, string text) where T : TextElement {
            element.text = text;
            return element;
        }

        public static T WhenClicked<T>(this T element, Action action) where T : Button {
            if (action == null) throw new ArgumentNullException(nameof(action));
            element.clickable.clicked += action;
            return element;
        }
    }
}