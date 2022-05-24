using System.Collections.Generic;
using System.Linq;
using CommandPalette.Utils;
using CommandPalette.Views;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommandPalette.Basic.Views {
    public class CommandParameterView : View {
        private const float PARAMETER_TITLE_HEIGHT = 64.0f;
        private const float PARAMETER_TITLE_SPACING = 6.0f;
        private const float PARAMETER_PADDING = 16.0f;
        private const float PARAMETER_SPACING = 6.0f;
        private const float PARAMETER_HEIGHT = 48.0f;
        private const float PARAMETER_EXECUTE_BUTTON_HEIGHT = 32.0f;
        private const int MAX_DISPLAYED_PARAMETER_COUNT = 6;

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
            titleContainer.style.height = PARAMETER_TITLE_HEIGHT;
            titleContainer.style.marginBottom = PARAMETER_TITLE_SPACING;
            this.m_ParametersContainer.Add(titleContainer);

            CommandParameterValues parameterValues = new CommandParameterValues(m_Entry.Parameters);
            this.m_ParametersContainer.userData = new object[]{ m_Entry, parameterValues };

            this.m_ParametersContainer.style.paddingTop = PARAMETER_PADDING;
            this.m_ParametersContainer.style.paddingBottom = PARAMETER_PADDING;
            this.m_ParametersContainer.style.paddingLeft = PARAMETER_PADDING;
            this.m_ParametersContainer.style.paddingRight = PARAMETER_PADDING;

            int displayedParameters = Mathf.Min(MAX_DISPLAYED_PARAMETER_COUNT, m_Entry.Parameters.Count(parameter => CommandPaletteParameterDriver.IsKnownType(parameter.Type)));
            float height = displayedParameters * PARAMETER_HEIGHT + displayedParameters * PARAMETER_SPACING + PARAMETER_TITLE_HEIGHT + PARAMETER_TITLE_SPACING + PARAMETER_EXECUTE_BUTTON_HEIGHT + 2.0f * PARAMETER_PADDING;

            Window.SetHeight(height);

            CreateParameterFields(parameterValues);

            this.m_ParametersContainer.Add(new Button(() => {
                m_Entry.Method.Invoke(null, parameterValues.Values);
                Window.Close();
            }).Initialized(button => {
                button.style.marginTop = PARAMETER_SPACING;
                button.style.height = PARAMETER_EXECUTE_BUTTON_HEIGHT;
            }).WithText("Execute").WithName("ExecuteEntryWithParameters"));
        }

        private void CreateParameterFields(CommandParameterValues parameterValues) {
            ScrollView scrollView = new ScrollView().WithClasses("parameters-scroll-view");
            this.m_ParametersContainer.Add(scrollView);

            int displayedParameters = Mathf.Min(MAX_DISPLAYED_PARAMETER_COUNT, parameterValues.Parameters.Count(parameter => CommandPaletteParameterDriver.IsKnownType(parameter.Type)));
            float height = displayedParameters * PARAMETER_HEIGHT + displayedParameters * PARAMETER_SPACING;
            scrollView.style.height = height;

            VisualElement firstField = null;
            List<int> unknownParameterTypes = new List<int>();
            for (int i = 0; i < parameterValues.Values.Length; i++) {
                if (CommandPaletteParameterDriver.IsKnownType(parameterValues.Parameters[i].Type)) {
                    VisualElement parameterField = CommandPaletteParameterDriver.CreateParameterField(parameterValues.Parameters[i].Type, parameterValues, i);
                    if (i == 0) {
                        firstField = parameterField;
                    } else {
                        parameterField.style.marginTop = PARAMETER_SPACING;
                    }

                    parameterField.style.height = PARAMETER_HEIGHT;

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