// Source: https://github.com/microsoft/PowerToys/blob/d66fac3c3c486a750743d7b1f240df001e1e9224/src/modules/launcher/Plugins/Microsoft.PowerToys.Run.Plugin.Calculator/CalculateEngine.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using Mages.Core;

namespace CommandPalette.Math {
    public class MathEngine {
        private readonly Engine m_MagesEngine = new Engine(new Configuration {
            Scope = new Dictionary<string, object> {
                { "e", System.Math.E }, // e is not contained in the default mages engine
            },
        });

        /// <summary>
        /// Interpret
        /// </summary>
        /// <param name="cultureInfo">Use CultureInfo.CurrentCulture if something is user facing</param>
        public CalculateResult? Interpret(string input, CultureInfo cultureInfo) {
            if (!MathPluginHelper.InputValid(input)) {
                return null;
            }

            // mages has quirky log representation
            // mage has log == ln vs log10
            input = input.Replace("log(", "log10(").Replace("ln(", "log(");

            object result = m_MagesEngine.Interpret(input);

            // This could happen for some incorrect queries, like pi(2)
            if (result == null) {
                return null;
            }

            if (string.IsNullOrEmpty(result?.ToString())) {
                return null;
            }

            decimal decimalResult = Convert.ToDecimal(result, cultureInfo);
            decimal roundedResult = Round(decimalResult, MathPlugin.Settings.DisplayDecimalPlaces);

            return new CalculateResult {
                Result = Round(decimalResult, MathPlugin.Settings.CopyDecimalPlaces),
                RoundedResult = roundedResult,
            };
        }

        public static decimal Round(decimal value, int digits) {
            return System.Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }
    }
}