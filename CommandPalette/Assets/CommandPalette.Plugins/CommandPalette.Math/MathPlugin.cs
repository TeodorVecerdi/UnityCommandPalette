using System;
using System.Collections.Generic;
using System.Globalization;
using CommandPalette.Core;
using CommandPalette.Math.Helpers;
using CommandPalette.Plugins;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Math {
    public class MathPlugin : IPlugin {
        [InitializeOnLoadMethod]
        private static void InitializePlugin() {
            CommandPalette.RegisterPlugin(new MathPlugin());
        }

        private static readonly MathEngine engine = new MathEngine();

        public float PriorityMultiplier { get; } = 2.0f;
        public CommandPaletteWindow Window { get; set; }

        public List<ResultEntry> GetResults(Query query) {
            if (!IsValid(query)) return new List<ResultEntry>();

            string text = query.Text.Trim();
            int priority = 100;

            if (text.StartsWith('=')) {
                text = text.Substring(1).Trim();
                priority = 10000;
            }

            try {
                // Using CurrentUICulture since this is user facing
                CalculateResult? result = engine.Interpret(text, CultureInfo.CurrentUICulture);

                // This could happen for some incorrect queries, like pi(2)
                if (!result.HasValue) {
                    return new List<ResultEntry>();
                }

                return new List<ResultEntry> {
                    CreateResult(result.Value, priority),
                };
            } // We want to keep the process alive if any the mages library throws any exceptions.
            catch (Exception e) {
                // Debug.LogException(new Exception($"Exception in math plugin with query '{query.Text}'", e));
            }

            return new List<ResultEntry>();
        }

        private MathResultEntry CreateResult(CalculateResult calculateResult, int priority) {
            return new MathResultEntry(new ResultDisplaySettings(calculateResult.RoundedResult.ToString(CultureInfo.CurrentCulture), "", "Copy to clipboard", IconResource.FromResource("MathPlugin/Textures/CalculatorIcon")), priority, CopyToClipboard) {UserData = calculateResult};
        }

        private static bool CopyToClipboard(ResultEntry result) {
            CalculateResult value = (CalculateResult) result.UserData;
            GUIUtility.systemCopyBuffer = value.Result.ToString(CultureInfo.CurrentCulture);

            return true;
        }

        public bool IsValid(Query query) {
            string text = query.Text.Trim();
            if (text.StartsWith('=')) {
                text = text.Substring(1).Trim();
            }

            return MathPluginHelper.InputValid(text);
        }
    }
}