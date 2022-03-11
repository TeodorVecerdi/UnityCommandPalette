using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FuzzySharp;
using FuzzySharp.Extractor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using CommandPalette.Commands;
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
        private const int k_MaxItemCount = 100;
        private const float k_ItemHeight = 64.0f;

        private const int k_SearchCutoff = 80;

        private static readonly int downSample = 1;
        private static readonly float blurSize = 1.3f;
        private static readonly int blurPasses = 32;
        private static readonly Color blurTintColor = Color.black;
        private static readonly float blurTintAmount = 0.1f;

        private static StyleSheet s_stylesheet;

        private bool m_ShouldQuit;
        private bool m_IsShowingParameters;
        private string m_SearchString = "";
        private List<(CommandEntry, int)> m_SearchResults;

        private VisualElement m_MainContainer;
        private TextField m_SearchField;
        private List<VisualElement> m_SearchResultElements;
        private ScrollView m_ResultsContainer;
        private VisualElement m_SelectedElement;
        private int m_SelectedIndex;

        private VisualElement m_ParametersContainer;

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
                if (evt.keyCode == KeyCode.Escape) {
                    Close();
                } else if (m_IsShowingParameters && evt.altKey && evt.keyCode == KeyCode.Backspace) {
                    SwitchToSearch();
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
                if (evt.keyCode == KeyCode.DownArrow) {
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

                    if (m_SelectedElement.userData is CommandEntry entry) {
                        ExecuteEntry(entry);
                    }
                }
            });
            this.m_MainContainer.Add(this.m_SearchField);

            m_ResultsContainer = new ScrollView(ScrollViewMode.Vertical).WithName("ResultsContainer");
            this.m_MainContainer.Add(m_ResultsContainer);
            this.m_SearchField.value = m_SearchString;

            this.m_ParametersContainer = new VisualElement().WithName("ParametersContainer").WithClasses("hidden");

            root.Add(this.m_MainContainer);
            root.Add(this.m_ParametersContainer);

            rootVisualElement.Add(root);
            rootVisualElement.schedule.Execute(() => {
                this.m_SearchField.hierarchy[0].Focus();
                position = new Rect(windowPosition.x + 0.5f * windowSize.x - 0.5f * k_BaseWidth, windowPosition.y + k_YOffset, k_BaseWidth, k_ClearHeight);
                UpdateResults();
            });

            /*rootVisualElement.style.overflow = Overflow.Visible;
            rootVisualElement.schedule.Execute(() => {
                IMGUIContainer imguiContainer = rootVisualElement.hierarchy.parent.Q<IMGUIContainer>();
                imguiContainer?.RemoveFromHierarchy();
                root.Add(new IMGUIContainer(OnGUI));
            });*/
        }

        private void UpdateResults() {
            if (string.IsNullOrWhiteSpace(m_SearchString)) {
                m_SearchResults = null;
                UpdateResultsView();
                return;
            }

            IEnumerable<ExtractedResult<string>> resultsDisplayName =
                Process.ExtractSorted(m_SearchString, CommandPaletteDriver.CommandEntries.Select(entry => entry.DisplayName), cutoff: k_SearchCutoff);
            IEnumerable<ExtractedResult<string>> resultsShortName =
                Process.ExtractSorted(m_SearchString, CommandPaletteDriver.CommandEntries.Select(entry => entry.ShortName), cutoff: k_SearchCutoff);

            Dictionary<int, ExtractedResult<string>> results = resultsDisplayName.ToDictionary(extractedResult => extractedResult.Index);

            foreach (ExtractedResult<string> extractedResult in resultsShortName) {
                if (!results.ContainsKey(extractedResult.Index)) {
                    results.Add(extractedResult.Index, extractedResult);
                } else {
                    if (results[extractedResult.Index].Score < extractedResult.Score) {
                        results[extractedResult.Index] = extractedResult;
                    }
                }
            }

            m_SearchResults = results.Select(keyValuePair => (CommandPaletteDriver.CommandEntries[keyValuePair.Key], keyValuePair.Value.Score)).ToList();
            m_SearchResults.Sort((t1, t2) => t2.Item2.CompareTo(t1.Item2));
            UpdateResultsView();
        }

        private void UpdateResultsView() {
            m_ResultsContainer.Clear();
            m_SelectedElement = null;
            List<CommandEntry> entries = null;
            if (m_SearchResults == null || m_SearchResults.Count == 0) {
                if (!string.IsNullOrWhiteSpace(m_SearchString) || CommandPaletteDriver.CommandEntries.Count == 0) {
                    m_ResultsContainer.AddToClassList("hidden");
                    Rect rect = position;
                    position = new Rect(rect.x, rect.y, rect.width, k_ClearHeight);
                } else {
                    entries = CommandPaletteDriver.CommandEntries.Take(k_MaxItemCount).ToList();
                    entries.RemoveAll(entry => entry.ValidationMethod != null && !(bool)entry.ValidationMethod.Invoke(null, null));

                    if (entries.Count > 0) {
                        int displayedCount = Math.Min(entries.Count, k_MaxDisplayedItemCount);
                        float extraHeight = displayedCount * k_ItemHeight + (displayedCount + 1) * k_ResultsSpacing;
                        Rect rect = position;
                        position = new Rect(rect.x, rect.y, rect.width, k_ClearHeight + extraHeight);
                    }
                }
            } else {
                entries = m_SearchResults.Take(k_MaxItemCount).Select(tuple => tuple.Item1).ToList();
                entries.RemoveAll(entry => entry.ValidationMethod != null && !(bool)entry.ValidationMethod.Invoke(null, null));

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
            foreach (CommandEntry entry in entries) {
                VisualElement resultElement = CreateResultElement(entry);

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

        private VisualElement CreateResultElement(CommandEntry commandEntry) {
            VisualElement resultElement = new VisualElement().WithClasses("search-result")
                                                             .WithUserData(commandEntry)
                                                             .Initialized(element => { element.style.height = k_ItemHeight; });
            VisualElement mainContainer = new VisualElement().WithClasses("search-result-main-container");
            mainContainer.Add(
                new VisualElement().WithClasses("search-result-title-container").WithChildren(
                    new Label(commandEntry.ShortName).WithClasses("search-result-short"),
                    new Label($"{commandEntry.DisplayName}").WithClasses("search-result-display")
                )
            );
            if (!string.IsNullOrWhiteSpace(commandEntry.Description)) {
                mainContainer.Add(new Label(commandEntry.Description).WithClasses("search-result-description"));
                resultElement.AddToClassList("has-description");
            }

            resultElement.Add(mainContainer);

            if (commandEntry.HasParameters) {
                resultElement.Add(new VisualElement().WithClasses("search-result-parameter-indicator"));
            }

            resultElement.AddManipulator(new Clickable(() => {
                ExecuteEntry(commandEntry);
            }));

            return resultElement;
        }

        private void ExecuteEntry(CommandEntry entry) {
            if (entry.HasParameters) {
                SwitchToParameterInput(entry);
            } else {
                entry.Method.Invoke(null, null);
                Close();
            }
        }

        private void SwitchToParameterInput(CommandEntry entry) {
            Debug.Log($"{entry.DisplayName}:\n{entry.Parameters.Dump("  ", "\n")}");
            this.m_IsShowingParameters = true;
            this.m_MainContainer.AddToClassList("hidden");
            this.m_ParametersContainer.RemoveFromClassList("hidden");

            LoadParameters(entry);
        }

        private void SwitchToSearch() {
            this.m_IsShowingParameters = false;

            this.m_MainContainer.RemoveFromClassList("hidden");
            this.m_ParametersContainer.AddToClassList("hidden");
            this.m_SearchField.hierarchy[0].Focus();
        }

        private void LoadParameters(CommandEntry entry) {
            this.m_ParametersContainer.Clear();

            CreateParameterFields(entry);
        }

        private void CreateParameterFields(CommandEntry entry) {
            CommandParameterValues parameterValues = new CommandParameterValues(entry.Parameters);
            this.m_ParametersContainer.userData = parameterValues;

            List<int> unknownParameterTypes = new List<int>();
            for (int i = 0; i < parameterValues.Values.Length; i++) {
                if (CommandPaletteParameterDriver.IsKnownType(parameterValues.CommandParameters.ParameterTypes[i])) {
                    VisualElement parameterField = CommandPaletteParameterDriver.CreateParameterField(parameterValues.CommandParameters.ParameterTypes[i], parameterValues, i);
                    this.m_ParametersContainer.Add(parameterField);
                } else {
                    unknownParameterTypes.Add(i);
                }
            }

            if (unknownParameterTypes.Count > 0) {
                VisualElement unknownParametersContainer = new VisualElement().WithClasses("unknown-parameters-container");
                unknownParametersContainer.Add(new Label("Unknown parameter types:"));
                foreach (int index in unknownParameterTypes) {
                    unknownParametersContainer.Add(new Label(parameterValues.CommandParameters.ParameterTypes[index].ToString()));
                }
                this.m_ParametersContainer.Add(unknownParametersContainer);
            }

            this.m_ParametersContainer.Add(new Button(() => {
                entry.Method.Invoke(null, parameterValues.Values);
                Close();
            }).WithText("Execute").WithName("ExecuteEntryWithParameters"));
        }

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

            if (current.isKey) {
                if (current.keyCode == KeyCode.Escape) {
                    m_ShouldQuit = true;
                }

                if (this.m_IsShowingParameters && current.alt && current.keyCode == KeyCode.Backspace) {
                    SwitchToSearch();
                }
            }
        }
    }
}