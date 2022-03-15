using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using CommandPalette.Core;
using CommandPalette.Plugins;
using CommandPalette.Utils;

namespace CommandPalette {
    public class CommandPaletteWindow : EditorWindow {
        private static Rect mainWindowRect;
        private static Vector2 windowPosition;
        private static Vector2Int windowSize;
        private static Texture2D backgroundTexture;
        private static Texture blurredTexture;

        [MenuItem("Tools/Destroy All Command Palettes")]
        private static void DestroyAllCommandPalettes() {
            CommandPaletteWindow[] windows = Resources.FindObjectsOfTypeAll<CommandPaletteWindow>();
            foreach (CommandPaletteWindow window in windows) {
                try {
                    window.Close();
                } catch {
                    DestroyImmediate(window);
                }
            }
        }

        [MenuItem("Tools/Command Palette &]")]
        private static void ShowWindow() {
            CommandPaletteWindow[] windows = Resources.FindObjectsOfTypeAll<CommandPaletteWindow>();
            foreach (CommandPaletteWindow commandPaletteWindow in windows) {
                commandPaletteWindow.Close();
            }

            CommandPaletteWindow window = CreateInstance<CommandPaletteWindow>();
            InitializeBackground();

            window.minSize = new Vector2(k_BaseWidth, k_ClearHeight);
            window.position = new Rect(windowPosition.x + 0.5f * windowSize.x - 0.5f * k_BaseWidth, windowPosition.y + k_YOffset, k_BaseWidth, k_ClearHeight);
            window.ShowPopup();
        }

        private static void InitializeBackground() {
            mainWindowRect = UnityExtensions.GetEditorMainWindowPos();
            windowPosition = mainWindowRect.position;
            windowSize = new Vector2Int((int)mainWindowRect.width, (int)mainWindowRect.height);

            if (backgroundTexture == null) {
                backgroundTexture = new Texture2D(windowSize.x, windowSize.y, TextureFormat.RGBA32, false, true);
            }

            Color[] screen = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(windowPosition, windowSize.x, windowSize.y);
            backgroundTexture.SetPixels(screen);
            backgroundTexture.Apply(false, false);

            if (blurredTexture != null) {
                DestroyImmediate(blurredTexture);
            }

            Blur blur = new Blur();
            blurredTexture = blur.BlurTexture(backgroundTexture, downSample, blurSize, blurPasses, blurTintColor, blurTintAmount);
        }

        private const float k_BaseWidth = 680.0f;
        private const float k_ClearHeight = 70.0f;
        private const float k_YOffset = 200.0f;

        private const float k_ResultsSpacing = 6.0f;
        private const int k_MaxDisplayedItemCount = 6;
        public const int k_MaxItemCount = 100;
        public const float k_ItemHeight = 64.0f;

        private const float k_ParameterTitleHeight = 64.0f;
        private const float k_ParameterTitleSpacing = 6.0f;
        private const float k_ParameterPadding = 16.0f;
        private const float k_ParameterSpacing = 6.0f;
        private const float k_ParameterHeight = 48.0f;
        private const float k_ParameterExecuteButtonHeight = 32.0f;
        private const int k_MaxDisplayedParameterCount = 6;

        private const int k_SearchCutoff = 80;

        private static readonly int downSample = 1;
        private static readonly float blurSize = 1.3f;
        private static readonly int blurPasses = 32;
        private static readonly Color blurTintColor = Color.black;
        private static readonly float blurTintAmount = 0.1f;

        private static StyleSheet s_stylesheet;

        private bool m_ShouldQuit;
        // private bool m_IsShowingParameters;
        private string m_SearchString = "";
        private List<ResultEntry> m_SearchResults;

        private VisualElement m_MainContainer;
        private TextField m_SearchField;
        private List<VisualElement> m_SearchResultElements;
        private ScrollView m_ResultsContainer;
        private VisualElement m_SelectedElement;
        private int m_SelectedIndex;

        // private VisualElement m_ParametersContainer;
        // private ScrollView m_ParametersScrollView;

        private void OnGUI() {
            ProcessEvents();
            if (m_ShouldQuit /* || focusedWindow != this*/) {
                Close();
                return;
            }
        }

        private void CreateGUI() {
            if (s_stylesheet == null) {
                LoadStylesheets();
            }

            VisualElement root = new VisualElement().WithName("CommandPalette").WithStylesheet(s_stylesheet);
            root.styleSheets.Add(s_stylesheet);
            root.Add(new IMGUIContainer(DrawTexture).WithName("Background"));
            root.RegisterCallback<KeyUpEvent>(evt => {
                if (evt.shiftKey && evt.keyCode == KeyCode.Escape) {
                    Close();
                } else if (evt.altKey && evt.keyCode == KeyCode.Backspace) {
                    // if (m_IsShowingParameters) {
                        // SwitchToSearch();
                    // } else {
                        Close();
                    // }
                }
            });

            this.m_MainContainer = new VisualElement().WithName("MainContainer");
            this.m_SearchField = new TextField().WithName("SearchField");
            this.m_SearchField.style.height = k_ClearHeight;
            Label placeholder = new Label("Start typing...").WithName("SearchPlaceholder").WithClassEnabled("hidden", !string.IsNullOrEmpty(m_SearchString));
            placeholder.pickingMode = PickingMode.Ignore;
            this.m_SearchField.Add(placeholder);
            this.m_SearchField.RegisterValueChangedCallback(evt => {
                m_SearchString = evt.newValue;
                placeholder.EnableInClassList("hidden", !string.IsNullOrEmpty(m_SearchString));

                UpdateResults();
            });
            this.m_SearchField.RegisterCallback<KeyDownEvent>(evt => {
                if (evt.keyCode == KeyCode.Escape) {
                    Close();
                } else if (evt.keyCode == KeyCode.DownArrow) {
                    SelectNext();
                    evt.PreventDefault();
                } else if (evt.keyCode == KeyCode.UpArrow) {
                    SelectPrevious();
                    evt.PreventDefault();
                } else if (evt.keyCode == KeyCode.Return) {
                    if (m_SelectedElement == null) {
                        evt.PreventDefault();
                        rootVisualElement.schedule.Execute(() => { this.m_SearchField.hierarchy[0].Focus(); });
                        return;
                    }

                    if (m_SelectedElement.userData is ResultEntry entry) {
                        ExecuteEntry(entry);
                    }
                }
            });
            this.m_MainContainer.Add(this.m_SearchField);

            m_ResultsContainer = new ScrollView(ScrollViewMode.Vertical).WithName("ResultsContainer");
            this.m_MainContainer.Add(m_ResultsContainer);
            this.m_SearchField.value = m_SearchString;

            // this.m_ParametersContainer = new VisualElement().WithName("ParametersContainer").WithClasses("hidden");

            root.Add(this.m_MainContainer);
            // root.Add(this.m_ParametersContainer);

            rootVisualElement.Add(root);
            rootVisualElement.schedule.Execute(() => {
                this.m_SearchField.hierarchy[0].Focus();
                position = new Rect(windowPosition.x + 0.5f * windowSize.x - 0.5f * k_BaseWidth, windowPosition.y + k_YOffset, k_BaseWidth, k_ClearHeight);
                UpdateResults();
            });
        }

        private void UpdateResults() {
            /*if (string.IsNullOrWhiteSpace(m_SearchString)) {
                m_SearchResults = null;
                UpdateResultsView();
                return;
            }*/
            m_SearchResults = PluginManager.GetResultsSorted(new Query(m_SearchString));
            UpdateResultsView();
        }

        private void UpdateResultsView() {
            m_ResultsContainer.Clear();
            m_SelectedElement = null;
            List<ResultEntry> entries = null;
            if (m_SearchResults == null || m_SearchResults.Count == 0) {
                m_ResultsContainer.AddToClassList("hidden");
                Rect rect = position;
                position = new Rect(rect.x, rect.y, rect.width, k_ClearHeight);
            } else {
                entries = m_SearchResults;
                if (entries.Count > 0) {
                    int displayedCount = Math.Min(entries.Count, k_MaxDisplayedItemCount);
                    float extraHeight = displayedCount * k_ItemHeight + (displayedCount + 1) * k_ResultsSpacing;
                    Rect rect = position;
                    position = new Rect(rect.x, rect.y, rect.width, k_ClearHeight + extraHeight);
                }
            }

            if (entries == null) {
                return;
            }

            if (entries.Count == 0) {
                m_ResultsContainer.AddToClassList("hidden");
                Rect rect = position;
                position = new Rect(rect.x, rect.y, rect.width, k_ClearHeight);
                return;
            }

            m_ResultsContainer.RemoveFromClassList("hidden");
            m_ResultsContainer.style.paddingTop = k_ResultsSpacing;
            m_ResultsContainer.style.paddingBottom = k_ResultsSpacing;
            m_SearchResultElements = new List<VisualElement>();

            int index = 0;
            foreach (ResultEntry entry in entries) {
                VisualElement resultElement = CommandPaletteUtility.CreateEntryElement(entry);
                resultElement.AddManipulator(new Clickable(() => {
                    ExecuteEntry(entry);
                }));


                if (index == 0) {
                    resultElement.AddToClassList("selected");
                    m_SelectedElement = resultElement;
                    m_SelectedIndex = 0;
                } else {
                    resultElement.style.marginTop = k_ResultsSpacing;
                }

                m_SearchResultElements.Add(resultElement);
                m_ResultsContainer.Add(resultElement);

                index++;
                if (index >= k_MaxItemCount) {
                    break;
                }
            }
        }

        private void SelectPrevious() {
            if (m_SearchResultElements == null || m_SearchResultElements.Count == 0) {
                return;
            }

            if (m_SelectedIndex != 0) {
                m_SelectedIndex--;
            } else {
                m_SelectedIndex = m_SearchResultElements.Count - 1;
            }

            m_SelectedElement.RemoveFromClassList("selected");
            m_SelectedElement = m_SearchResultElements[m_SelectedIndex];
            m_SelectedElement.AddToClassList("selected");
            m_ResultsContainer.ScrollTo(m_SelectedElement);
        }

        private void SelectNext() {
            if (m_SearchResultElements == null || m_SearchResultElements.Count == 0) {
                return;
            }

            if (m_SelectedIndex != m_SearchResultElements.Count - 1) {
                m_SelectedIndex++;
            } else {
                m_SelectedIndex = 0;
            }

            m_SelectedElement.RemoveFromClassList("selected");
            m_SelectedElement = m_SearchResultElements[m_SelectedIndex];
            m_SelectedElement.AddToClassList("selected");
            m_ResultsContainer.ScrollTo(m_SelectedElement);
        }

        private void ExecuteEntry(ResultEntry entry) {
            if (entry.OnSelect(entry)) {
                Close();
            }
        }

        /*
        private void SwitchToParameterInput(CommandEntry entry) {
            this.m_IsShowingParameters = true;
            this.m_MainContainer.AddToClassList("hidden");
            this.m_ParametersContainer.RemoveFromClassList("hidden");

            LoadParameters(entry);
        }
        */

        /*private void SwitchToSearch() {
            this.m_IsShowingParameters = false;

            this.m_ParametersContainer.Clear();
            this.m_MainContainer.RemoveFromClassList("hidden");
            this.m_ParametersContainer.AddToClassList("hidden");
            this.m_SearchField.hierarchy[0].Focus();
        }*/

        /*
        private void LoadParameters(CommandEntry entry) {
            this.m_ParametersContainer.Clear();
            VisualElement titleContainer = new VisualElement().WithClasses("search-result-main-container", "parameter-title");
            titleContainer.Add(
                new VisualElement().WithClasses("search-result-title-container", "parameter-title-title-container").WithChildren(
                    new Label(entry.ShortName).WithClasses("search-result-short", "parameter-title-short"),
                    new Label($"{entry.DisplayName}").WithClasses("search-result-display", "parameter-title-display")
                )
            );
            if (!string.IsNullOrWhiteSpace(entry.Description)) {
                titleContainer.Add(new Label(entry.Description).WithClasses("search-result-description", "parameter-title-description"));
                titleContainer.AddToClassList("has-description");
            }
            titleContainer.style.height = k_ParameterTitleHeight;
            titleContainer.style.marginBottom = k_ParameterTitleSpacing;
            this.m_ParametersContainer.Add(titleContainer);

            CommandParameterValues parameterValues = new CommandParameterValues(entry.Parameters);
            this.m_ParametersContainer.userData = new object[]{ entry, parameterValues };

            this.m_ParametersContainer.style.paddingTop = k_ParameterPadding;
            this.m_ParametersContainer.style.paddingBottom = k_ParameterPadding;
            this.m_ParametersContainer.style.paddingLeft = k_ParameterPadding;
            this.m_ParametersContainer.style.paddingRight = k_ParameterPadding;

            int displayedParameters = Mathf.Min(k_MaxDisplayedParameterCount, entry.Parameters.Count(parameter => CommandPaletteParameterDriver.IsKnownType(parameter.Type)));
            float height = displayedParameters * k_ParameterHeight + displayedParameters * k_ParameterSpacing + k_ParameterTitleHeight + k_ParameterTitleSpacing + k_ParameterExecuteButtonHeight + 2.0f * k_ParameterPadding;

            Rect rect = position;
            position = new Rect(rect.x, rect.y, rect.width, height);

            CreateParameterFields(parameterValues);

            this.m_ParametersContainer.Add(new Button(() => {
                entry.Method.Invoke(null, parameterValues.Values);
                Close();
            }).Initialized(button => {
                button.style.marginTop = k_ParameterSpacing;
                button.style.height = k_ParameterExecuteButtonHeight;
            }).WithText("Execute").WithName("ExecuteEntryWithParameters"));
        }
        */

        /*
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
                                Close();
                            }
                        }
                    });
                    scrollView.Add(parameterField);
                } else {
                    unknownParameterTypes.Add(i);
                }
            }

            rootVisualElement.schedule.Execute(() => {
                firstField?.Focus();
            });
        }
        */

        private void DrawTexture() {
            if (blurredTexture == null) {
                ReloadBackgroundTexture();
            }

            Rect currentPosition = position;
            int offsetX = -(int)(currentPosition.x - windowPosition.x);
            int offsetY = -(int)(currentPosition.y - windowPosition.y);
            GUI.DrawTextureWithTexCoords(new Rect(offsetX, offsetY, windowSize.x, windowSize.y), blurredTexture, new Rect(0, 0, 1, 1));
        }

        private void ReloadBackgroundTexture() {
            Rect oldPosition = position;
            position = Rect.zero;
            InitializeBackground();
            position = oldPosition;
        }

        private static void LoadStylesheets() {
            s_stylesheet = Resources.Load<StyleSheet>("CommandPalette/Stylesheets/CommandPalette");
        }

        private void ProcessEvents() {
            Event current = Event.current;
            if (current == null) {
                return;
            }

            if (current.isKey && current.type == EventType.KeyUp) {
                if (current.shift && current.keyCode == KeyCode.Escape) {
                    m_ShouldQuit = true;
                } else if (current.alt && current.keyCode == KeyCode.Backspace) {
                    // if (m_IsShowingParameters) {
                        // SwitchToSearch();
                    // } else {
                        m_ShouldQuit = true;
                    // }
                }
            }
        }
    }
}