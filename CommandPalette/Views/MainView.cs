﻿using System;
using System.Collections.Generic;
using CommandPalette.Core;
using CommandPalette.Plugins;
using CommandPalette.Settings;
using CommandPalette.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommandPalette.Views {
    public sealed class MainView : View {
        private const float k_ResultsSpacing = 6.0f;
        private const int k_MaxDisplayedItemCount = 6;

        public const float SEARCH_FIELD_HEIGHT = 70.0f;
        public const int MAX_ITEM_COUNT = 100;
        public const float ITEM_HEIGHT = 64.0f;

        private static string s_SearchString = "";
        private static CommandPaletteSettings s_Settings;

        private List<ResultEntry> m_SearchResults;

        private VisualElement m_MainContainer;
        private TextField m_SearchField;
        private List<VisualElement> m_SearchResultElements;
        private ScrollView m_ResultsContainer;
        private VisualElement m_SelectedElement;
        private Label m_NoResultsLabel;
        private int m_SelectedIndex;

        public override void OnEvent(Event evt) {
            if (evt.isKey && evt.type == EventType.KeyUp) {
                if (evt.shift && evt.keyCode == KeyCode.Escape) {
                    Window.Close();
                } else if (evt.alt && evt.keyCode == KeyCode.Backspace) {
                    Window.Close();
                }
            }
        }

        public override VisualElement Build() {
            if (s_Settings == null) {
                s_Settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            m_MainContainer = new VisualElement().WithName("MainContainer");
            m_SearchField = new TextField().WithName("SearchField");
            Label placeholder = new Label("Start typing...").WithName("SearchPlaceholder").WithClassEnabled("hidden", !string.IsNullOrEmpty(s_SearchString));
            placeholder.pickingMode = PickingMode.Ignore;
            m_SearchField.Add(placeholder);
            m_SearchField.RegisterValueChangedCallback(evt => {
                s_SearchString = evt.newValue;
                placeholder.EnableInClassList("hidden", !string.IsNullOrEmpty(s_SearchString));
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

                    if (m_SelectedElement.userData is ResultEntry entry) {
                        ExecuteEntry(entry);
                    }
                }
            });
            m_MainContainer.Add(m_SearchField);

            m_NoResultsLabel = new Label("No Results Found").WithName("NoResultsLabel").WithClasses("hidden");
            m_MainContainer.Add(m_NoResultsLabel);

            m_ResultsContainer = new ScrollView(ScrollViewMode.Vertical).WithName("ResultsContainer");
            m_MainContainer.Add(m_ResultsContainer);
            m_SearchField.value = s_SearchString;

            m_MainContainer.schedule.Execute(() => {
                m_SearchField.hierarchy[0].Focus();
                UpdateResults();
            });

            return m_MainContainer;
        }

        private void UpdateResults() {
            m_SearchResults = PluginManager.GetResultsSorted(new Query(s_SearchString));
            UpdateResultsView();
        }

        private void UpdateResultsView() {
            m_ResultsContainer.Clear();
            m_SelectedElement = null;
            if (m_SearchResults == null || m_SearchResults.Count == 0) {
                m_NoResultsLabel.RemoveFromClassList("hidden");
                m_ResultsContainer.AddToClassList("hidden");
                Window.SetHeight(SEARCH_FIELD_HEIGHT);
                return;
            }

            List<ResultEntry> entries = m_SearchResults;
            if (entries.Count > 0) {
                int displayedCount = Math.Min(entries.Count, k_MaxDisplayedItemCount);
                float extraHeight = displayedCount * ITEM_HEIGHT + (displayedCount + 1) * k_ResultsSpacing;
                Window.SetHeight(SEARCH_FIELD_HEIGHT + extraHeight);
            }

            if (entries.Count == 0) {
                m_NoResultsLabel.RemoveFromClassList("hidden");
                m_ResultsContainer.AddToClassList("hidden");
                Window.SetHeight(SEARCH_FIELD_HEIGHT);
                return;
            }

            m_NoResultsLabel.AddToClassList("hidden");
            m_ResultsContainer.RemoveFromClassList("hidden");
            m_ResultsContainer.style.paddingTop = k_ResultsSpacing;
            m_ResultsContainer.style.paddingBottom = k_ResultsSpacing;
            m_SearchResultElements = new List<VisualElement>();

            int index = 0;
            foreach (ResultEntry entry in entries) {
                VisualElement resultElement = CommandPaletteUtility.CreateEntryElement(entry);
                resultElement.AddManipulator(new Clickable(() => { ExecuteEntry(entry); }));

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

            m_SelectedElement?.RemoveFromClassList("selected");
            m_SelectedElement = m_SearchResultElements[m_SelectedIndex];
            m_SelectedElement?.AddToClassList("selected");
            if (m_SelectedElement != null && m_ResultsContainer.Contains(m_SelectedElement)) {
                m_ResultsContainer.ScrollTo(m_SelectedElement);
            }
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

            m_SelectedElement?.RemoveFromClassList("selected");
            m_SelectedElement = m_SearchResultElements[m_SelectedIndex];
            m_SelectedElement?.AddToClassList("selected");
            if (m_SelectedElement != null && m_ResultsContainer.Contains(m_SelectedElement)) {
                m_ResultsContainer.ScrollTo(m_SelectedElement);
            }
        }

        private void ExecuteEntry(ResultEntry entry) {
            if (entry.OnSelect?.Invoke(entry) ?? false) {
                if (s_Settings.ClearSearchOnSelection) {
                    s_SearchString = "";
                }

                Window.Close();
            }
        }
    }
}