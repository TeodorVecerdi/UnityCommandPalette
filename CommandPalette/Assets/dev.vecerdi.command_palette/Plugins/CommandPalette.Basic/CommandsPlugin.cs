using System;
using System.Collections.Generic;
using System.Linq;
using CommandPalette.Basic.Settings;
using CommandPalette.Basic.Views;
using CommandPalette.Core;
using CommandPalette.Plugins;
using CommandPalette.Settings;
using CommandPalette.Views;
using FuzzySharp;
using FuzzySharp.Extractor;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Basic {
    public class CommandsPlugin : IPlugin, IPluginSettingsProvider {
        [InitializeOnLoadMethod]
        private static void InitializePlugin() {
            CommandsPlugin commandsPlugin = new CommandsPlugin();
            CommandPalette.RegisterPlugin(commandsPlugin);
            s_settings = CommandPalette.GetSettings<CommandsPluginSettings>(commandsPlugin);
        }

        private const string PARAMETER_SUFFIX_TEXTURE_PATH = "CommandPalette.Basic/Textures/right-chevron";

        private static CommandsPluginSettings s_settings;

        public string Name { get; } = "Basic Commands";
        public float PriorityMultiplier { get; } = 1.0f;
        public CommandPaletteWindow Window { get; set; }

        public IEnumerable<ResultEntry> GetResults(Query query) {
            if (string.IsNullOrWhiteSpace(query.Text)) {
                if (CommandPaletteDriver.CommandEntries.Count == 0) {
                    return new List<ResultEntry>();
                }

                List<ResultEntry> results = new List<ResultEntry>();

                int count = 0;
                foreach (CommandEntry entry in CommandPaletteDriver.CommandEntries) {
                    if(entry.ShowOnlyWhenSearching || (entry.ValidationMethod != null && !(bool)entry.ValidationMethod.Invoke(null, null))) {
                        continue;
                    }

                    results.Add(CommandToResult(entry, 0));
                    count++;

                    if (count >= MainView.MAX_ITEM_COUNT) {
                        break;
                    }
                }

                return results;
            }

            List<CommandEntry> validCommands = CommandPaletteDriver.CommandEntries.Where(entry => entry.ValidationMethod == null || (bool)entry.ValidationMethod.Invoke(null, null)).ToList();

            IEnumerable<ExtractedResult<string>> resultsDisplayName =
                Process.ExtractSorted(query.Text, validCommands.Select(entry => entry.DisplayName), cutoff: s_settings.SearchCutoff);
            IEnumerable<ExtractedResult<string>> resultsShortName =
                Process.ExtractSorted(query.Text, validCommands.Select(entry => entry.ShortName), cutoff: s_settings.SearchCutoff);
            Dictionary<int, ExtractedResult<string>> resultDictionary = resultsDisplayName.ToDictionary(extractedResult => extractedResult.Index);

            foreach (ExtractedResult<string> extractedResult in resultsShortName) {
                if (!resultDictionary.ContainsKey(extractedResult.Index)) {
                    resultDictionary.Add(extractedResult.Index, extractedResult);
                } else {
                    if (resultDictionary[extractedResult.Index].Score < extractedResult.Score) {
                        resultDictionary[extractedResult.Index] = extractedResult;
                    }
                }
            }

            List<(CommandEntry, int)> searchResults = resultDictionary.Select(keyValuePair => (validCommands[keyValuePair.Key], keyValuePair.Value.Score)).ToList();
            searchResults.Sort((t1, t2) => t2.Item2.CompareTo(t1.Item2));

            return searchResults.Select(tuple => CommandToResult(tuple.Item1, tuple.Item2));
        }

        private ResultEntry CommandToResult(CommandEntry commandEntry, int score) {
            return new ResultEntry(
                new ResultDisplaySettings(commandEntry.DisplayName, commandEntry.ShortName, commandEntry.Description, commandEntry.Icon,
                                          commandEntry.HasParameters ? IconResource.FromResource(PARAMETER_SUFFIX_TEXTURE_PATH) : default), score,
                entry => ExecuteEntry((CommandEntry)entry.UserData)) { UserData = commandEntry };
        }

        public bool IsValid(Query query) => true;

        private bool ExecuteEntry(CommandEntry entry) {
            if (entry.HasParameters) {
                if (entry.HasInlineSupport) {
                    Window.SwitchToView<InlineParameterValueView>(view => view.Initialize(entry));
                } else {
                    Window.SwitchToView<CommandParameterView>(view => view.Entry = entry);
                }
                return false;
            }

            entry.Method.Invoke(null, null);
            return true;
        }

        public Type SettingsType { get; } = typeof(CommandsPluginSettings);

        public void AddKeywords(HashSet<string> keywords) {
            keywords.Add("Search Cutoff");
        }

        public void DrawSettings(SerializedObject settings) {
            EditorGUILayout.PropertyField(settings.FindProperty("m_SearchCutoff"));
            settings.ApplyModifiedProperties();
        }
    }
}