using CommandPalette.Core;
using CommandPalette.Utils;
using CommandPalette.Views;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommandPalette {
    public static class CommandPaletteUtility {
        public static VisualElement CreateEntryElement(ResultEntry entry) {
            VisualElement resultElement = new VisualElement().WithClasses("result-entry")
                                                             .WithUserData(entry)
                                                             .Initialized(element => { element.style.height = MainView.k_ItemHeight; });

            VisualElement mainContainer = new VisualElement().WithClasses("result-entry-main-container");
            mainContainer.Add(
                new VisualElement().WithClasses("result-entry-title-container").WithChildren(
                    new Label(entry.DisplaySettings.ShortName).WithClasses("result-entry-short"),
                    new Label(entry.DisplaySettings.Title).WithClasses("result-entry-display")
                )
            );
            if (!string.IsNullOrWhiteSpace(entry.DisplaySettings.Description)) {
                mainContainer.Add(new Label(entry.DisplaySettings.Description).WithClasses("result-entry-description"));
                resultElement.AddToClassList("has-description");
            }

            if(!string.IsNullOrWhiteSpace(entry.DisplaySettings.Icon)) {
                resultElement.AddToClassList("has-icon");
                VisualElement iconElement = new VisualElement().WithClasses("result-entry-icon");
                Texture2D icon = entry.DisplaySettings.Icon.StartsWith("r:")
                    ? EditorGUIUtility.IconContent(entry.DisplaySettings.Icon[2..]).image as Texture2D
                    : Resources.Load<Texture2D>(entry.DisplaySettings.Icon);
                iconElement.style.backgroundImage = new StyleBackground(icon);
                resultElement.Add(iconElement);
            }

            resultElement.Add(mainContainer);

            if (!string.IsNullOrWhiteSpace(entry.DisplaySettings.SuffixIcon)) {
                resultElement.AddToClassList("has-suffix-icon");
                VisualElement suffixIconElement = new VisualElement().WithClasses("result-entry-suffix-icon");
                Texture2D icon = entry.DisplaySettings.SuffixIcon.StartsWith("r:")
                    ? EditorGUIUtility.IconContent(entry.DisplaySettings.SuffixIcon[2..]).image as Texture2D
                    : Resources.Load<Texture2D>(entry.DisplaySettings.SuffixIcon);
                suffixIconElement.style.backgroundImage = new StyleBackground(icon);
                resultElement.Add(suffixIconElement);
            }

            entry.PostProcessVisualElement(resultElement);

            return resultElement;
        }
    }
}