using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CommandPalette.Colors {
    public static class ColorHelper {
        private static readonly Regex s_RgbRegex  = new Regex(@"^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$", RegexOptions.Compiled);
        private static readonly Regex s_RgbaRegex = new Regex(@"^rgba\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$", RegexOptions.Compiled);
        private static readonly Regex s_HsvRegex  = new Regex(@"^hsv\((\d{1,3}(?:\.\d+)?),\s*(\d{1,3}(?:\.\d+)?)(%?),\s*(\d{1,3}(?:\.\d+)?)(%?)\)$", RegexOptions.Compiled);
        private static readonly Regex s_HsvaRegex = new Regex(@"^hsv\((\d{1,3}(?:\.\d+)?),\s*(\d{1,3}(?:\.\d+)?)(%?),\s*(\d{1,3}(?:\.\d+)?)(%?),\s*(\d{1,3}(?:\.\d+)?)(%?)\)$", RegexOptions.Compiled);
        private static readonly Regex s_HslRegex  = new Regex(@"^hsl\((\d{1,3}(?:\.\d+)?),\s*(\d{1,3}(?:\.\d+)?)(%?),\s*(\d{1,3}(?:\.\d+)?)(%?)\)$", RegexOptions.Compiled);
        private static readonly Regex s_HslaRegex = new Regex(@"^hsl\((\d{1,3}(?:\.\d+)?),\s*(\d{1,3}(?:\.\d+)?)(%?),\s*(\d{1,3}(?:\.\d+)?)(%?),\s*(\d{1,3}(?:\.\d+)?)(%?)\)$", RegexOptions.Compiled);
        private static readonly Regex s_Hex3Regex = new Regex(@"^#([a-fA-F0-9]{1})([a-fA-F0-9]{1})([a-fA-F0-9]{1})$", RegexOptions.Compiled);
        private static readonly Regex s_Hex4Regex = new Regex(@"^#([a-fA-F0-9]{1})([a-fA-F0-9]{1})([a-fA-F0-9]{1})([a-fA-F0-9]{1})$", RegexOptions.Compiled);
        private static readonly Regex s_Hex6Regex = new Regex(@"^#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})$", RegexOptions.Compiled);
        private static readonly Regex s_Hex8Regex = new Regex(@"^#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})$", RegexOptions.Compiled);

        public static bool IsValid(string query) {
            query = query.Trim();

            return  s_RgbRegex.IsMatch(query) ||
                   s_RgbaRegex.IsMatch(query) ||
                    s_HsvRegex.IsMatch(query) ||
                   s_HsvaRegex.IsMatch(query) ||
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

            if (s_HsvRegex.IsMatch(query)) {
                return ExtractHsv(query);
            }

            if (s_HsvaRegex.IsMatch(query)) {
                return ExtractHsva(query);
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

        private static Color ExtractHsv(string query) {
            Match match = s_HsvRegex.Match(query);
            float h, s, v;

            h = Mathf.Clamp(float.Parse(match.Groups[1].Value), 0.0f, 360.0f) / 360f;
            bool isSaturationPercentage = match.Groups[3].Value == "%";
            bool isValuePercentage = match.Groups[5].Value == "%";

            if (isSaturationPercentage) {
                s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 100.0f) / 100f;
            } else {
                s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 1.0f);
            }

            if (isValuePercentage) {
                v = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 100.0f) / 100f;
            } else {
                v = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 1.0f);
            }

            return Color.HSVToRGB(h, s, v);
        }

        private static Color ExtractHsva(string query) {
            Match match = s_HsvaRegex.Match(query);
            float h, s, v, a;

            h = Mathf.Clamp(float.Parse(match.Groups[1].Value), 0.0f, 360.0f) / 360f;
            bool isSaturationPercentage = match.Groups[3].Value == "%";
            bool isValuePercentage = match.Groups[5].Value == "%";
            bool isAlphaPercentage = match.Groups[7].Value == "%";

            if (isSaturationPercentage) {
                s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 100.0f) / 100f;
            } else {
                s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 1.0f);
            }

            if (isValuePercentage) {
                v = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 100.0f) / 100f;
            } else {
                v = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 1.0f);
            }

            if (isAlphaPercentage) {
                a = Mathf.Clamp(float.Parse(match.Groups[6].Value), 0.0f, 100.0f) / 100f;
            } else {
                a = Mathf.Clamp(float.Parse(match.Groups[6].Value), 0.0f, 1.0f);
            }

            Color color = Color.HSVToRGB(h, s, v);
            color.a = a;
            return color;
        }

        private static Color ExtractHsl(string query) {
            Match match = s_HslRegex.Match(query);
            float h, s, l;

            h = Mathf.Clamp(float.Parse(match.Groups[1].Value), 0.0f, 360.0f) / 360f;
            bool isSaturationPercentage = match.Groups[3].Value == "%";
            bool isLightnessPercentage = match.Groups[5].Value == "%";

            if (isSaturationPercentage) {
                s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 100.0f) / 100f;
            } else {
                s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 1.0f);
            }

            if (isLightnessPercentage) {
                l = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 100.0f) / 100f;
            } else {
                l = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 1.0f);
            }

            return HslToRgb(h, s, l);
        }

        private static Color ExtractHsla(string query) {
            Match match = s_HslaRegex.Match(query);
            float h, s, l, a;

            h = Mathf.Clamp(float.Parse(match.Groups[1].Value), 0.0f, 360.0f) / 360f;
            bool isSaturationPercentage = match.Groups[3].Value == "%";
            bool isLightnessPercentage = match.Groups[5].Value == "%";
            bool isAlphaPercentage = match.Groups[7].Value == "%";

            if (isSaturationPercentage) {
                s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 100.0f) / 100f;
            } else {
                s = Mathf.Clamp(float.Parse(match.Groups[2].Value), 0.0f, 1.0f);
            }

            if (isLightnessPercentage) {
                l = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 100.0f) / 100f;
            } else {
                l = Mathf.Clamp(float.Parse(match.Groups[4].Value), 0.0f, 1.0f);
            }

            if (isAlphaPercentage) {
                a = Mathf.Clamp(float.Parse(match.Groups[6].Value), 0.0f, 100.0f) / 100f;
            } else {
                a = Mathf.Clamp(float.Parse(match.Groups[6].Value), 0.0f, 1.0f);
            }

            Color color = HslToRgb(h, s, l);
            color.a = a;
            return color;
        }

        internal static Color HslToRgb(float h, float s, float l) {
            float r, g, b;

            if(s == 0.0f){
                r = g = b = l; // achromatic
            } else {
                float Hue2RGB(float p, float q, float t) {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    return t switch {
                        < 0.1666667F => p + (q - p) * 6.0f * t,
                        < 0.5F => q,
                        < 0.6666667F => p + (q - p) * (0.6666667F - t) * 6.0f,
                        _ => p
                    };
                }

                float q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                float p = 2 * l - q;
                r = Hue2RGB(p, q, h + 0.3333333F);
                g = Hue2RGB(p, q, h);
                b = Hue2RGB(p, q, h - 0.3333333F);
            }

            return new Color(r, g, b);
        }

        internal static void RgbToHsl(Color color, out float h, out float s, out float l) {
            float r = color.r;
            float g = color.g;
            float b = color.b;
            float max = Mathf.Max(r, g, b);
            float min = Mathf.Min(r, g, b);
            l = (max + min) / 2;

            if(Math.Abs(max - min) < 0.0001f){
                h = s = 0.0f; // achromatic
            } else {
                float d = max - min;
                s = l > 0.5f ? d / (2.0f - max - min) : d / (max + min);
                if (Math.Abs(max - r) < 0.00001f) {
                    h = (g - b) / d + (g < b ? 6.0f : 0.0f);
                } else if (Math.Abs(max - g) < 0.00001f) {
                    h = (b - r) / d + 2.0f;
                } else {
                    h = (r - g) / d + 4.0f;
                }

                h /= 6.0f;
            }
        }
    }
}