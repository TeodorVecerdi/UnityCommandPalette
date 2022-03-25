using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CommandPalette.Colors {
    public static class ColorHelper {
        private static readonly Regex s_RgbRegex  = new Regex(@"^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$", RegexOptions.Compiled);
        private static readonly Regex s_RgbaRegex = new Regex(@"^rgba\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$", RegexOptions.Compiled);
        private static readonly Regex s_HslRegex  = new Regex(@"^hsl\((\d{1,3}(?:\.\d+)?),\s*(\d{1,3}(?:\.\d+)?)%?,\s*(\d{1,3}(?:\.\d+)?)%?\)$", RegexOptions.Compiled);
        private static readonly Regex s_HslaRegex = new Regex(@"^hsla\((\d{1,3}(?:\.\d+)?),\s*(\d{1,3}(?:\.\d+)?)%?,\s*(\d{1,3}(?:\.\d+)?)%?,\s*(\d{1,3}(?:\.\d+)?)%?\)$", RegexOptions.Compiled);
        private static readonly Regex s_Hex3Regex = new Regex(@"^#([a-fA-F0-9]{1})([a-fA-F0-9]{1})([a-fA-F0-9]{1})$", RegexOptions.Compiled);
        private static readonly Regex s_Hex4Regex = new Regex(@"^#([a-fA-F0-9]{1})([a-fA-F0-9]{1})([a-fA-F0-9]{1})([a-fA-F0-9]{1})$", RegexOptions.Compiled);
        private static readonly Regex s_Hex6Regex = new Regex(@"^#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})$", RegexOptions.Compiled);
        private static readonly Regex s_Hex8Regex = new Regex(@"^#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})$", RegexOptions.Compiled);

        public static bool IsValid(string query) {
            query = query.Trim();

            return  s_RgbRegex.IsMatch(query) ||
                   s_RgbaRegex.IsMatch(query) ||
                    s_HslRegex.IsMatch(query) ||
                   s_HslaRegex.IsMatch(query) ||
                   s_Hex3Regex.IsMatch(query) ||
                   s_Hex4Regex.IsMatch(query) ||
                   s_Hex6Regex.IsMatch(query) ||
                   s_Hex8Regex.IsMatch(query);
        }

        public static Color? Extract(string query) {
            query = query.Trim();

            if (s_RgbRegex.IsMatch(query)) {
                return ExtractRgb(query);
            }

            if (s_RgbaRegex.IsMatch(query)) {
                return ExtractRgba(query);
            }

            if (s_HslRegex.IsMatch(query)) {
                return ExtractHsl(query);
            }

            if (s_HslaRegex.IsMatch(query)) {
                return ExtractHsla(query);
            }

            if (s_Hex3Regex.IsMatch(query)) {
                return ExtractHex(query);
            }

            if (s_Hex4Regex.IsMatch(query)) {
                return ExtractHex(query);
            }

            if (s_Hex6Regex.IsMatch(query)) {
                return ExtractHex(query);
            }

            if (s_Hex8Regex.IsMatch(query)) {
                return ExtractHex(query);
            }

            return null;
        }

        private static Color ExtractHex(string query) {
            string hex = query.Substring(1);

            if (hex.Length == 3) hex += "F";
            if (hex.Length == 6) hex += "FF";

            if (hex.Length == 4) {
                hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2], hex[3], hex[3]);
            }

            if (hex.Length != 8) {
                throw new ArgumentException("Invalid hexadecimal color string");
            }

            return new Color32(
                byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber)
            );
        }

        private static Color ExtractRgb(string query) {
            Match match = s_RgbRegex.Match(query);

            return new Color32(
                byte.Parse(match.Groups[1].Value),
                byte.Parse(match.Groups[2].Value),
                byte.Parse(match.Groups[3].Value),
                byte.MaxValue
            );
        }

        private static Color ExtractRgba(string query) {
            Match match = s_RgbaRegex.Match(query);

            return new Color32(
                byte.Parse(match.Groups[1].Value),
                byte.Parse(match.Groups[2].Value),
                byte.Parse(match.Groups[3].Value),
                byte.Parse(match.Groups[4].Value)
            );
        }

        private static Color ExtractHsl(string query) {
            Match match = s_HslRegex.Match(query);

            float h = Mathf.Clamp(float.Parse(match.Groups[1].Value), 0.0f, 360.0f) / 360f;
            float s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 100.0f) / 100f;
            float l = Mathf.Clamp(float.Parse(match.Groups[3].Value), 0.0f, 100.0f) / 100f;

            return Color.HSVToRGB(h, s, l);
        }

        private static Color ExtractHsla(string query) {
            Match match = s_HslaRegex.Match(query);

            float h = Mathf.Clamp(float.Parse(match.Groups[1].Value), 0.0f, 360.0f) / 360f;
            float s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 100.0f) / 100f;
            float l = Mathf.Clamp(float.Parse(match.Groups[3].Value), 0.0f, 100.0f) / 100f;
            float a = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 100.0f) / 100f;

            Color color = Color.HSVToRGB(h, s, l);
            color.a = a;
            return color;
        }
    }
}