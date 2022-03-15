using System.Collections.Generic;
using System.Linq;
using CommandPalette.Core;
using CommandPalette.Plugins;
using FuzzySharp;
using FuzzySharp.Extractor;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Commands {
    public class BasicCommandPlugin : IPlugin {
        [InitializeOnLoadMethod]
        private static void InitializePlugin() {
            CommandPalette.RegisterPlugin(new BasicCommandPlugin());
        }

        private const string k_ParameterSuffixTexturePath = "CommandPalette/Textures/right-chevron";
        private const int k_SearchCutoff = 80;

        public float PriorityMultiplier { get; } = 1.0f;

        public List<ResultEntry> GetResults(Query query) {
            if (string.IsNullOrWhiteSpace(query.Text)) {
                if (CommandPaletteDriver.CommandEntries.Count == 0) {
                    return new List<ResultEntry>();
                }

                List<ResultEntry> results = new List<ResultEntry>();

                foreach (CommandEntry entry in CommandPaletteDriver.CommandEntries.Take(CommandPaletteWindow.k_MaxItemCount)) {
                    if(entry.ValidationMethod != null && !(bool)entry.ValidationMethod.Invoke(null, null)) {
                        continue;
                    }

                    results.Add(CommandToResult(entry, 0));
                }

                return results;
            }

            IEnumerable<ExtractedResult<string>> resultsDisplayName =
                Process.ExtractSorted(query.Text, CommandPaletteDriver.CommandEntries.Select(entry => entry.DisplayName), cutoff: k_SearchCutoff);
            IEnumerable<ExtractedResult<string>> resultsShortName =
                Process.ExtractSorted(query.Text, CommandPaletteDriver.CommandEntries.Select(entry => entry.ShortName), cutoff: k_SearchCutoff);
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

            List<(CommandEntry, int)> searchResults = resultDictionary.Select(keyValuePair => (CommandPaletteDriver.CommandEntries[keyValuePair.Key], keyValuePair.Value.Score)).ToList();
            searchResults.Sort((t1, t2) => t2.Item2.CompareTo(t1.Item2));

            List<ResultEntry> resultEntries = new List<ResultEntry>();
            foreach ((CommandEntry commandEntry, int score) in searchResults) {
                resultEntries.Add(
                    CommandToResult(commandEntry, score));
            }

            return resultEntries;
        }

        private static ResultEntry CommandToResult(CommandEntry commandEntry, int score) {
            return new ResultEntry(
                new ResultDisplaySettings(commandEntry.DisplayName, commandEntry.ShortName, commandEntry.Description, "r:d_unitylogo",
                                          commandEntry.HasParameters ? k_ParameterSuffixTexturePath : null), score,
                entry => ExecuteEntry((CommandEntry)entry.UserData)) { UserData = commandEntry };
        }

        public bool IsValid(Query query) => true;

        private static bool ExecuteEntry(CommandEntry entry) {
            if (entry.HasParameters) {
                Debug.Log($"Executing {entry.DisplayName} with parameters (not implemented)");
                return false;
                //SwitchToParameterInput(entry);
            }

            entry.Method.Invoke(null, null);
            return true;
        }
    }
}