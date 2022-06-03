using System;
using System.Collections.Generic;
using System.Linq;
using CommandPalette.Core;
using CommandPalette.Utils;
using CommandPalette.Views;
using FuzzySharp;
using FuzzySharp.Extractor;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommandPalette.Basic {
    public sealed class InlineParameterValueView : View {
        private const float SEARCH_FIELD_HEIGHT = 100.0f;
        private const float RESULTS_SPACING = 6.0f;
        private const int MAX_DISPLAYED_ITEM_COUNT = 6;

        public const int MAX_ITEM_COUNT = 100;
        public const float ITEM_HEIGHT = 64.0f;
        private const int SEARCH_CUTOFF = 60;

        private string m_SearchString = "";
        private List<InlineParameterResultEntry> m_SearchResults;

        private VisualElement m_MainContainer;
        private TextField m_SearchField;
        private List<VisualElement> m_SearchResultElements;
        private ScrollView m_ResultsContainer;
        private VisualElement m_SelectedElement;
        private int m_SelectedIndex;

        private CommandEntry m_Entry;
        private InlineParameterValues m_InlineParameterResults;

        public void Initialize(CommandEntry entry) {
            m_Entry = entry;
            m_InlineParameterResults = (InlineParameterValues) entry.Parameters[0].InlineValuesProvider.Invoke(null, null);
            foreach (InlineParameterResultEntry inlineParameterResult in m_InlineParameterResults) {
                inlineParameterResult.OnSelect += ExecuteEntry;
            }
        }

        public override void OnEvent(Event evt) {
            if (evt.isKey && evt.type == EventType.KeyUp) {
                if (evt.shift && evt.keyCode == KeyCode.Escape) {
                    Window.Close();
                } else if (evt.alt && evt.keyCode == KeyCode.Backspace) {
                    Window.SwitchToView<MainView>();
                }
            }
        }

        public override VisualElement Build() {
            m_MainContainer = new VisualElement().WithName("MainContainer");
            m_SearchField = new TextField().WithName("SearchField");
            m_SearchField.style.height = SEARCH_FIELD_HEIGHT;
            Label placeholder = new Label("Start typing...").WithName("SearchPlaceholder").WithClassEnabled("hidden", !string.IsNullOrEmpty(m_SearchString));
            placeholder.pickingMode = PickingMode.Ignore;
            m_SearchField.Add(placeholder);
            m_SearchField.RegisterValueChangedCallback(evt => {
                m_SearchString = evt.newValue;
                placeholder.EnableInClassList("hidden", !string.IsNullOrEmpty(m_SearchString));
                UpdateResults();
            });

            m_SearchField.RegisterCallback<KeyDownEvent>(evt => {
                if (evt.keyCode == KeyCode.Escape) {
                    Window.Close();
                } else if (evt.keyCode == KeyCode.DownArrow) {
                    SelectNext();
                    evt.PreventDefault();
                } else if (evt.keyCode == KeyCode.UpArrow) {
                    SelectPrevious();
                    evt.PreventDefault();
                } else if (evt.keyCode == KeyCode.Return) {
                    if (m_SelectedElement == null) {
                        evt.PreventDefault();
                        m_MainContainer.schedule.Execute(() => { m_SearchField.hierarchy[0].Focus(); });
                        return;
                    }

                    if (m_SelectedElement.userData is InlineParameterResultEntry entry) {
                        ExecuteEntry(entry);
                    }
                } else if (evt.altKey && evt.keyCode == KeyCode.Backspace) {
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                    Window.SwitchToView<MainView>();
                }
            });

            m_MainContainer.Add(m_SearchField);

            m_ResultsContainer = new ScrollView(ScrollViewMode.Vertical).WithName("ResultsContainer");
            m_MainContainer.Add(m_ResultsContainer);
            m_SearchField.value = m_SearchString;

            m_MainContainer.schedule.Execute(() => {
                m_SearchField.hierarchy[0].Focus();
                UpdateResults();
            });

            return m_MainContainer;
        }

        private void UpdateResults() {
            m_SearchResults = GetSearchResults(m_SearchString);
            UpdateResultsView();
        }

        private List<InlineParameterResultEntry> GetSearchResults(string query) {
            if (string.IsNullOrWhiteSpace(query)) {
                return m_InlineParameterResults;
            }

            IEnumerable<ExtractedResult<InlineParameterResultEntry>> results =
                Process.ExtractSorted(
                    new InlineParameterResultEntry(null, new ResultDisplaySettings(query)),
                    m_InlineParameterResults, entry => entry.DisplaySettings.Title, cutoff: SEARCH_CUTOFF, scorer: ScorerCache.Get<PartialRatioScorer>());

            List<InlineParameterResultEntry> entries = results.Select(result => result.Value).ToList();
            if (entries.Count == 0) {
                entries = Process.ExtractSorted(
                    new InlineParameterResultEntry(null, new ResultDisplaySettings(query)),
                    m_InlineParameterResults, entry => entry.DisplaySettings.Title, ScorerCache.Get<PartialRatioScorer>()).Select(result => result.Value).ToList();
            }

            return entries;
        }

        private void UpdateResultsView() {
            m_ResultsContainer.Clear();
            m_SelectedElement = null;
            if (m_SearchResults == null || m_SearchResults.Count == 0) {
                m_ResultsContainer.AddToClassList("hidden");
                Window.SetHeight(SEARCH_FIELD_HEIGHT);
                return;
            }

            List<InlineParameterResultEntry> entries = m_SearchResults;
            if (entries.Count > 0) {
                int displayedCount = Math.Min(entries.Count, MAX_DISPLAYED_ITEM_COUNT);
                float extraHeight = displayedCount * ITEM_HEIGHT + (displayedCount + 1) * RESULTS_SPACING;
                Window.SetHeight(SEARCH_FIELD_HEIGHT + extraHeight);
            }

            if (entries.Count == 0) {
                m_ResultsContainer.AddToClassList("hidden");
                Window.SetHeight(SEARCH_FIELD_HEIGHT);
                return;
            }

            m_ResultsContainer.RemoveFromClassList("hidden");
            m_ResultsContainer.style.paddingTop = RESULTS_SPACING;
            m_ResultsContainer.style.paddingBottom = RESULTS_SPACING;
            m_SearchResultElements = new List<VisualElement>();

            int index = 0;
            foreach (InlineParameterResultEntry entry in entries) {
                VisualElement resultElement = CommandPaletteUtility.CreateEntryElement(entry);
                resultElement.AddManipulator(new Clickable(() => { ExecuteEntry(entry); }));

                if (index == 0) {
                    resultElement.AddToClassList("selected");
                    m_SelectedElement = resultElement;
                    m_SelectedIndex = 0;
                } else {
                    resultElement.style.marginTop = RESULTS_SPACING;
                }

                m_SearchResultElements.Add(resultElement);
                m_ResultsContainer.Add(resultElement);

                index++;
                if (index >= MAX_ITEM_COUNT) {
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

        private void ExecuteEntry(InlineParameterResultEntry entry) {
            m_Entry.Method.Invoke(null, new [] { entry.Value });
            Window.Close();
        }
    }
}