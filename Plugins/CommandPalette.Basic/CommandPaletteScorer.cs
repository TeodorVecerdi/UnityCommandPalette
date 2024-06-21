#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FuzzySharp;
using FuzzySharp.Extractor;

namespace CommandPalette.Basic {
    public static class CommandPaletteScorer {
        private const float kPrefixScoreMultiplier = 2.0f;
        public static IEnumerable<ExtractedResult<string>> ScoreResults(string query, int cutoff, IEnumerable<string> choices)
            => ScoreResults(query, cutoff, choices, StringProcessor);

        public static IEnumerable<ExtractedResult<T>> ScoreResults<T>(T query, int cutoff, IEnumerable<T> choices, Func<T, string> processor) {
            string processedQuery = processor(query);
            return Process.ExtractSorted(query, choices, processor, cutoff: cutoff)
                          .Select(result => {
                              if (string.IsNullOrWhiteSpace(processedQuery)) {
                                  return result;
                              }

                              string processedValue = processor(result.Value);
                              if (!processedValue.StartsWith(processedQuery, StringComparison.InvariantCultureIgnoreCase)) {
                                  return result;
                              }

                              return new ExtractedResult<T>(result.Value, (int)(result.Score * kPrefixScoreMultiplier), result.Index);
                          })
                          .OrderByDescending(result => result.Score);
        }

        private static readonly Regex s_StringProcessor = new("[^ a-zA-Z0-9]");
        private static string StringProcessor(string input) {
            input = s_StringProcessor.Replace(input, " ");
            input = input.ToLower();
            return input.Trim();
        }
    }
}
