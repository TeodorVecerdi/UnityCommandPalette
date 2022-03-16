﻿// Source: https://github.com/microsoft/PowerToys/blob/d66fac3c3c486a750743d7b1f240df001e1e9224/src/modules/launcher/Plugins/Microsoft.PowerToys.Run.Plugin.Calculator/CalculateEngine.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using Mages.Core;
using CommandPalette.MathPlugin.Helpers;
using UnityEngine;

namespace CommandPalette.MathPlugin {
    public class MathEngine {
        private readonly Engine _magesEngine = new Engine(new Configuration {
            Scope = new Dictionary<string, object> {
                { "e", Math.E }, // e is not contained in the default mages engine
            },
        });

        public const int RoundingDigits = 10;

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

            object result = _magesEngine.Interpret(input);

            // This could happen for some incorrect queries, like pi(2)
            if (result == null) {
                return null;
            }

            if (string.IsNullOrEmpty(result?.ToString())) {
                return null;
            }

            decimal decimalResult = Convert.ToDecimal(result, cultureInfo);
            decimal roundedResult = Round(decimalResult);

            return new CalculateResult() {
                Result = decimalResult,
                RoundedResult = roundedResult,
            };
        }

        public static decimal Round(decimal value) {
            return Math.Round(value, RoundingDigits, MidpointRounding.AwayFromZero);
        }
    }
}