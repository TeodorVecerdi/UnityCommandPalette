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
                                                             .Initialized(element => { element.style.height = MainView.ITEM_HEIGHT; });

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

            if(!Equals(entry.DisplaySettings.Icon, default(IconResource))) {
                resultElement.AddToClassList("has-icon");
                VisualElement iconElement = new VisualElement().WithClasses("result-entry-icon");
                iconElement.style.backgroundImage = new StyleBackground(entry.DisplaySettings.Icon.GetTexture(entry.ResourcePathProvider) as Texture2D);
                resultElement.Add(iconElement);
            }

            resultElement.Add(mainContainer);

            if (!Equals(entry.DisplaySettings.SuffixIcon, default(IconResource))) {
                resultElement.AddToClassList("has-suffix-icon");
                VisualElement suffixIconElement = new VisualElement().WithClasses("result-entry-suffix-icon");
                suffixIconElement.style.backgroundImage = new StyleBackground(entry.DisplaySettings.SuffixIcon.GetTexture(entry.ResourcePathProvider) as Texture2D);
                resultElement.Add(suffixIconElement);
            }

            entry.PostProcessVisualElement(resultElement);

            return resultElement;
        }
    }
}