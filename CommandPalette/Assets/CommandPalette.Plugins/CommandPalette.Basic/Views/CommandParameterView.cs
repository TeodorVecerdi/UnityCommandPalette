using System.Collections.Generic;
using System.Linq;
using CommandPalette.Utils;
using CommandPalette.Views;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommandPalette.Basic.Views {
    public class CommandParameterView : View {
        private const float k_ParameterTitleHeight = 64.0f;
        private const float k_ParameterTitleSpacing = 6.0f;
        private const float k_ParameterPadding = 16.0f;
        private const float k_ParameterSpacing = 6.0f;
        private const float k_ParameterHeight = 48.0f;
        private const float k_ParameterExecuteButtonHeight = 32.0f;
        private const int k_MaxDisplayedParameterCount = 6;

        private VisualElement m_ParametersContainer;
        private CommandEntry m_Entry;

        public CommandEntry Entry {
            get => m_Entry;
            set => m_Entry = value;
        }

        public override void OnEvent(Event evt) {
            if (evt.alt && evt.keyCode == KeyCode.Backspace) {
                Window.SwitchToView<MainView>();
            }
        }

        public override VisualElement Build() {
            m_ParametersContainer = new VisualElement().WithName("ParametersContainer");
            LoadParameters();
            return m_ParametersContainer;
        }

        private void LoadParameters() {
            this.m_ParametersContainer.Clear();
            VisualElement titleContainer = new VisualElement().WithClasses("result-entry-main-container", "parameter-title");
            titleContainer.Add(
                new VisualElement().WithClasses("result-entry-title-container", "parameter-title-title-container").WithChildren(
                    new Label(m_Entry.ShortName).WithClasses("result-entry-short", "parameter-title-short"),
                    new Label($"{m_Entry.DisplayName}").WithClasses("result-entry-display", "parameter-title-display")
                )
            );
            if (!string.IsNullOrWhiteSpace(m_Entry.Description)) {
                titleContainer.Add(new Label(m_Entry.Description).WithClasses("result-entry-description", "parameter-title-description"));
                titleContainer.AddToClassList("has-description");
            }
            titleContainer.style.height = k_ParameterTitleHeight;
            titleContainer.style.marginBottom = k_ParameterTitleSpacing;
            this.m_ParametersContainer.Add(titleContainer);

            CommandParameterValues parameterValues = new CommandParameterValues(m_Entry.Parameters);
            this.m_ParametersContainer.userData = new object[]{ m_Entry, parameterValues };

            this.m_ParametersContainer.style.paddingTop = k_ParameterPadding;
            this.m_ParametersContainer.style.paddingBottom = k_ParameterPadding;
            this.m_ParametersContainer.style.paddingLeft = k_ParameterPadding;
            this.m_ParametersContainer.style.paddingRight = k_ParameterPadding;

            int displayedParameters = Mathf.Min(k_MaxDisplayedParameterCount, m_Entry.Parameters.Count(parameter => CommandPaletteParameterDriver.IsKnownType(parameter.Type)));
            float height = displayedParameters * k_ParameterHeight + displayedParameters * k_ParameterSpacing + k_ParameterTitleHeight + k_ParameterTitleSpacing + k_ParameterExecuteButtonHeight + 2.0f * k_ParameterPadding;

            Window.SetHeight(height);

            CreateParameterFields(parameterValues);

            this.m_ParametersContainer.Add(new Button(() => {
                m_Entry.Method.Invoke(null, parameterValues.Values);
                Window.Close();
            }).Initialized(button => {
                button.style.marginTop = k_ParameterSpacing;
                button.style.height = k_ParameterExecuteButtonHeight;
            }).WithText("Execute").WithName("ExecuteEntryWithParameters"));
        }

        private void CreateParameterFields(CommandParameterValues parameterValues) {
            ScrollView scrollView = new ScrollView().WithClasses("parameters-scroll-view");
            this.m_ParametersContainer.Add(scrollView);

            int displayedParameters = Mathf.Min(k_MaxDisplayedParameterCount, parameterValues.Parameters.Count(parameter => CommandPaletteParameterDriver.IsKnownType(parameter.Type)));
            float height = displayedParameters * k_ParameterHeight + displayedParameters * k_ParameterSpacing;
            scrollView.style.height = height;

            VisualElement firstField = null;
            List<int> unknownParameterTypes = new List<int>();
            for (int i = 0; i < parameterValues.Values.Length; i++) {
                if (CommandPaletteParameterDriver.IsKnownType(parameterValues.Parameters[i].Type)) {
                    VisualElement parameterField = CommandPaletteParameterDriver.CreateParameterField(parameterValues.Parameters[i].Type, parameterValues, i);
                    if (i == 0) {
                        firstField = parameterField;
                    } else {
                        parameterField.style.marginTop = k_ParameterSpacing;
                    }

                    parameterField.style.height = k_ParameterHeight;

                    parameterField.RegisterCallback<KeyDownEvent>(evt => {
                        if (evt.keyCode == KeyCode.Return && evt.altKey) {
                            evt.StopImmediatePropagation();
                            evt.PreventDefault();

                            if (this.m_ParametersContainer.userData is object[] userData) {
                                CommandEntry entry = (CommandEntry)userData[0];
                                entry.Method.Invoke(null, ((CommandParameterValues)userData[1]).Values);
                                Window.Close();
                            }
                        }
                    });
                    scrollView.Add(parameterField);
                } else {
                    unknownParameterTypes.Add(i);
                }
            }

            m_ParametersContainer.schedule.Execute(() => {
                firstField?.Focus();
            });
        }
    }
}