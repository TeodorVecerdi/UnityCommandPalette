using System;
using System.Text.RegularExpressions;

// Source: https://github.com/microsoft/PowerToys/blob/d66fac3c3c486a750743d7b1f240df001e1e9224/src/modules/launcher/Plugins/Microsoft.PowerToys.Run.Plugin.Calculator/CalculateHelper.cs

namespace CommandPalette.Math.Helpers {
    internal static class MathPluginHelper {
        private static readonly Regex s_validExpressChar = new Regex(
            @"^(" +
            @"%|" +
            @"ceil\s*\(|floor\s*\(|exp\s*\(|max\s*\(|min\s*\(|abs\s*\(|log\s*\(|ln\s*\(|sqrt\s*\(|pow\s*\(|" +
            @"factorial\s*\(|sign\s*\(|round\s*\(|rand\s*\(|" +
            @"sin\s*\(|cos\s*\(|tan\s*\(|arcsin\s*\(|arccos\s*\(|arctan\s*\(|" +
            @"sinh\s*\(|cosh\s*\(|tanh\s*\(|arsinh\s*\(|arcosh\s*\(|artanh\s*\(|" +
            @"pi|" +
            @"==|~=|&&|\|\||" +
            @"e|[0-9]|0x[0-9a-fA-F]+|0b[01]+|[\+\-\*\/\^\., ""]|[\(\)\|\!\[\]]" +
            @")+$", RegexOptions.Compiled);

        public static bool InputValid(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                return false;
            }

            bool singleDigitFactorial = input.EndsWith("!", StringComparison.InvariantCulture);
            if (input.Length <= 2 && !singleDigitFactorial) {
                return false;
            }

            if (!s_validExpressChar.IsMatch(input)) {
                return false;
            }

            if (!BracketHelper.IsBracketComplete(input)) {
                return false;
            }

            // If the input ends with a binary operator then it is not a valid input to mages and the Interpret function would throw an exception.
            string trimmedInput = input.TrimEnd();
            if (trimmedInput.EndsWith('+') || trimmedInput.EndsWith('-') || trimmedInput.EndsWith('*') || trimmedInput.EndsWith('|') || trimmedInput.EndsWith('\\') ||
                trimmedInput.EndsWith('^') || trimmedInput.EndsWith('=') || trimmedInput.EndsWith('&')) {
                return false;
            }

            return true;
        }
    }
}