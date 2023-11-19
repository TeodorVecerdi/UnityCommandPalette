using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CommandPalette.Core;
using CommandPalette.Plugins;
using CommandPalette.Resource;
using CommandPalette.Views;
using FuzzySharp;
using FuzzySharp.Extractor;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Basic {
    public partial class CommandsPlugin : IPlugin, IResourcePathProvider {
        [InitializeOnLoadMethod]
        private static void InitializePlugin() {
            CommandPalette.RegisterPlugin(s_Plugin);
            s_Settings = CommandPalette.GetSettings(s_Plugin);
        }

        private const string k_ParameterSuffixTexturePath = "Textures/right-chevron.png";

        private static readonly CommandsPlugin s_Plugin = new();
        private static CommandsPluginSettings s_Settings;

        public static IResourcePathProvider ResourcePathProvider => s_Plugin;

        public string Name => "Basic Commands";
        public float PriorityMultiplier => 1.0f;
        public CommandPaletteWindow Window { get; set; }

        public IEnumerable<ResultEntry> GetResults(Query query) {
            if (string.IsNullOrWhiteSpace(query.Text)) {
                if (CommandPaletteDriver.CommandEntries.Count == 0) {
                    return new List<ResultEntry>();
                }

                List<ResultEntry> results = new();

                int count = 0;
                foreach (CommandEntry entry in CommandPaletteDriver.CommandEntries) {
                    if(entry.ShowOnlyWhenSearching || (entry.ValidationMethod != null && !(bool)entry.ValidationMethod.Invoke(null, null))) {
                        continue;
                    }

                    results.Add(CommandToResult(entry, (int)(10.0f * entry.Priority)));
                    count++;

                    if (count >= MainView.MAX_ITEM_COUNT) {
                        break;
                    }
                }

                return results;
            }

            List<CommandEntry> validCommands = CommandPaletteDriver.CommandEntries.Where(entry => entry.ValidationMethod == null || (bool)entry.ValidationMethod.Invoke(null, null)).ToList();

            IEnumerable<ExtractedResult<string>> resultsDisplayName =
                Process.ExtractSorted(query.Text, validCommands.Select(entry => entry.DisplayName), cutoff: s_Settings.SearchCutoff);
            IEnumerable<ExtractedResult<string>> resultsShortName =
                Process.ExtractSorted(query.Text, validCommands.Select(entry => entry.ShortName), cutoff: s_Settings.SearchCutoff);
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

            List<(CommandEntry Command, int Score)> searchResults = resultDictionary.Select(keyValuePair => {
                CommandEntry command = validCommands[keyValuePair.Key];
                return (Command: command, Score: Mathf.CeilToInt(command.Priority * keyValuePair.Value.Score));
            }).OrderByDescending(t => t.Score).ToList();

            return searchResults.Select(tuple => CommandToResult(tuple.Command, tuple.Score));
        }

        private ResultEntry CommandToResult(CommandEntry commandEntry, int score) {
            return new ResultEntry(
                new ResultDisplaySettings(commandEntry.DisplayName, commandEntry.ShortName, commandEntry.Description, commandEntry.Icon,
                                          commandEntry.HasParameters ? IconResource.FromResource(k_ParameterSuffixTexturePath) : default), score,
                entry => ExecuteEntry((CommandEntry)entry.UserData!), ResourcePathProvider) { UserData = commandEntry };
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

        public string GetResourcePath(string path) {
            return Path.Combine(Path.GetDirectoryName(PathHelper())!.Replace("\\", "/").Replace(Application.dataPath, "Assets"), "EditorResources", path).Replace("\\", "/");
        }

        private static string PathHelper([CallerFilePath] string path = "") => path;
    }
}